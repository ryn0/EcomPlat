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

        public ProductsController(ApplicationDbContext context)
        {
            this.context = context;
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

            // Apply category filtering (if provided).
            if (!string.IsNullOrWhiteSpace(categoryKey))
            {
                query = query.Where(p => p.Subcategory.Category.CategoryKey == categoryKey);

                if (!string.IsNullOrWhiteSpace(subCategoryKey))
                {
                    query = query.Where(p => p.Subcategory.SubcategoryKey == subCategoryKey);
                }
            }

            // Apply search filter (if search term is provided)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearchTerm) || p.Description.ToLower().Contains(lowerSearchTerm));
            }

            // Apply sorting
            if (string.IsNullOrWhiteSpace(sortOrder) || sortOrder == "name")
            {
                query = query.OrderBy(p => p.Name);
            }
            else if (sortOrder == "priceAsc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sortOrder == "priceDesc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            // Pagination
            int totalProducts = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Rewrite image URLs for each product.
            var config = await this.GetImageUrlConfigAsync();
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

            var config = await this.GetImageUrlConfigAsync();
            foreach (var image in product.Images)
            {
                image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
            }

            return this.View("DetailsByKey", product);
        }

        private async Task<(string cdnPrefix, string blobPrefix)> GetImageUrlConfigAsync()
        {
            var cdnSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.CdnPrefixWithProtocol);
            var blobSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.BlobPrefix);

            string cdnPrefix = cdnSetting?.Content?.TrimEnd('/') ?? "";
            string blobPrefix = blobSetting?.Content?.TrimEnd('/') ?? "";
            return (cdnPrefix, blobPrefix);
        }
    }
}