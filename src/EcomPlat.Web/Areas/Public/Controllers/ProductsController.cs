using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.Constants;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("products")]
    public class ProductsController : Controller
    {
        private const int DefaultPageSize = 50;
        private readonly ApplicationDbContext context;

        public ProductsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /products or /products/index
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize)
        {
            if (page < 1) { page = 1; }
            if (pageSize < 1) { pageSize = 10; }

            // Query only available products.
            var query = this.context.Products
                .Include(p => p.Images)
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Include(p => p.Company)
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name);

            int totalProducts = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Retrieve config settings from the database.
            var config = await this.GetImageUrlConfigAsync();

            // Rewrite image URLs for each product.
            foreach (var product in products)
            {
                foreach (var image in product.Images)
                {
                    image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
                }
            }

            this.ViewData["CurrentPage"] = page;
            this.ViewData["PageSize"] = pageSize;
            this.ViewData["TotalProducts"] = totalProducts;

            return this.View(products);
        }

        [HttpGet("/product/{productKey}")]
        public async Task<IActionResult> DetailsByKey(string productKey)
        {
            // Find the product by productKey.
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

            // Retrieve config settings from the database.
            var config = await this.GetImageUrlConfigAsync();

            // Rewrite image URLs.
            foreach (var image in product.Images)
            {
                image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
            }

            return this.View("DetailsByKey", product);
        }

        /// <summary>
        /// Retrieves the CDN and blob URL prefixes from the ConfigSettings table.
        /// </summary>
        /// <returns>A tuple containing the CDN prefix and Blob prefix.</returns>
        private async Task<(string cdnPrefix, string blobPrefix)> GetImageUrlConfigAsync()
        {
            // Assuming you have a DbSet<ConfigSetting> ConfigSettings in your context and a SiteConfigSetting enum.
            var cdnSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.CdnPrefixWithProtocol);
            var blobSetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.BlobPrefix);

            // Trim any trailing slash for consistency.
            string cdnPrefix = cdnSetting?.Content?.TrimEnd('/') ?? "";
            string blobPrefix = blobSetting?.Content?.TrimEnd('/') ?? "";
            return (cdnPrefix, blobPrefix);
        }
    }
}
