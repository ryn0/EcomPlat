using DirectoryManager.Web.Services.Interfaces;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("products/{categoryKey?}/{subCategoryKey?}")]
    public class ProductsController : Controller
    {
        private const int DefaultPageSize = 50;
        private readonly ApplicationDbContext context;
        private readonly ICacheService cacheService;

        public ProductsController(
            ApplicationDbContext context,
            ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string categoryKey,
            string subCategoryKey,
            string searchTerm = "",
            int page = 1,
            int pageSize = DefaultPageSize,
            string sortOrder = "")
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            // Get IDs of subcategories with available products.
            var availableSubcategoryIds = await this.context.Products
                .Where(p => p.IsAvailable)
                .Select(p => p.SubcategoryId)
                .Distinct()
                .ToListAsync();

            // Load all categories with their subcategories (This is NOT affected by search!)
            var allCategories = await this.context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Ensure that all categories remain in ViewBag.
            var availableCategories = allCategories
                .Select(c => new
                {
                    Category = c,
                    Subcategories = c.Subcategories.Where(sc => availableSubcategoryIds.Contains(sc.SubcategoryId)).ToList()
                })
                .Where(x => x.Subcategories.Any())
                .Select(x =>
                {
                    x.Category.Subcategories = x.Subcategories;
                    return x.Category;
                })
                .ToList();

            // Base query: only available products.
            var query = this.context.Products
                .Include(p => p.Images.Where(x => x.IsMain == true))
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Include(p => p.Company)
                .Where(p => p.IsAvailable);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Bypass category/subcategory filters when searching
                string lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearchTerm) || p.Description.ToLower().Contains(lowerSearchTerm));
            }
            else
            {
                // Only apply category filtering if no search is being performed
                if (!string.IsNullOrWhiteSpace(categoryKey))
                {
                    query = query.Where(p => p.Subcategory.Category.CategoryKey == categoryKey);

                    if (!string.IsNullOrWhiteSpace(subCategoryKey))
                    {
                        query = query.Where(p => p.Subcategory.SubcategoryKey == subCategoryKey);
                    }
                }
            }

            // Apply sorting
            query = SortQuery(sortOrder, query);

            // Pagination
            int totalProducts = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Rewrite image URLs for each product.
            var config = this.GetImageUrlConfig();
            foreach (var product in products)
            {
                foreach (var image in product.Images)
                {
                    image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
                }
            }

            // Store data for the view.
            this.ViewData["SortOrder"] = sortOrder;
            this.ViewData["CurrentPage"] = page;
            this.ViewData["PageSize"] = pageSize;
            this.ViewData["TotalProducts"] = totalProducts;
            this.ViewData["SelectedCategoryKey"] = categoryKey;
            this.ViewData["SelectedSubCategoryKey"] = subCategoryKey;
            this.ViewData["SearchTerm"] = searchTerm;

            this.ViewBag.AllCategories = availableCategories; // Keep categories visible!

            return this.View("Index", products);
        }

        [HttpGet("/product/{productKey}")]
        public async Task<IActionResult> DetailsByKey(string productKey)
        {
            var product = await this.context.Products
                .Include(p => p.Images)
                .Include(p => p.Company)
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsAvailable);

            if (product == null)
            {
                return this.NotFound();
            }

            var config = this.GetImageUrlConfig();
            foreach (var image in product.Images)
            {
                image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
            }

            return this.View("DetailsByKey", product);
        }

        private static IQueryable<Data.Models.Product> SortQuery(string sortOrder, IQueryable<Data.Models.Product> query)
        {
            switch (sortOrder)
            {
                case "priceAsc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "weightAsc":
                    query = query.OrderBy(p => p.ProductWeightOunces);
                    break;
                case "weightDesc":
                    query = query.OrderByDescending(p => p.ProductWeightOunces);
                    break;
                case "productReviewDesc":
                    query = query.OrderByDescending(p => p.ProductReview);
                    break;
                case "productReviewAsc":
                    query = query.OrderBy(p => p.ProductReview);
                    break;
                case "name":
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            return query;
        }

        private (string cdnPrefix, string blobPrefix) GetImageUrlConfig()
        {
            var cdnSetting = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
            var blobSetting = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            return Converters.UrlConverters.ConvertCdnUrls(cdnSetting, blobSetting);
        }
    }
}