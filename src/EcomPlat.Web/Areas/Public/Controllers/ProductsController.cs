using EcomPlat.Data.DbContextInfo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProductsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /products or /products/index
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            if (page < 1) { page = 1; }
            if (pageSize < 1) { pageSize = 10; }

            // Query only available products.
            var query = this.context.Products
                .Include(p => p.Images)  // Include images
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Include(p => p.Company)
                .Where(p => p.IsAvailable);


            int totalProducts = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            this.ViewData["CurrentPage"] = page;
            this.ViewData["PageSize"] = pageSize;
            this.ViewData["TotalProducts"] = totalProducts;

            return this.View(products);
        }

        [HttpGet("/product/{productKey}")]
        public async Task<IActionResult> DetailsByKey(string productKey)
        {
            // Find the product by productKey
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

            return this.View("DetailsByKey", product);
        }

        [HttpPost]
        public async Task<JsonResult> SetMainImageAjax(int imageId, string productKey)
        {
            // Find the chosen image.
            var chosenImage = await this.context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId);
            if (chosenImage == null)
            {
                return this.Json(new { success = false, message = "Image not found." });
            }

            // Find the product using productKey.
            var product = await this.context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductKey == productKey);
            if (product == null)
            {
                return this.Json(new { success = false, message = "Product not found." });
            }

            // Unmark all images for the product.
            foreach (var img in product.Images)
            {
                img.IsMain = false;
            }
            // Mark all variants in the chosen image's group as main.
            var chosenGroupGuid = chosenImage.ImageGroupGuid;
            foreach (var img in product.Images.Where(i => i.ImageGroupGuid == chosenGroupGuid))
            {
                img.IsMain = true;
            }
            await this.context.SaveChangesAsync();

            // Get the updated main large image (if available, otherwise fallback to any large image).
            var mainLarge = product.Images.FirstOrDefault(i => i.IsMain && i.Size == EcomPlat.Data.Enums.ImageSize.Large)
                             ?? product.Images.FirstOrDefault(i => i.Size == EcomPlat.Data.Enums.ImageSize.Large);

            return this.Json(new { success = true, mainImageUrl = mainLarge?.ImageUrl });
        }

    }
}
