using EcomPlat.Data.DbContextInfo;
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

    }
}
