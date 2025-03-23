using DirectoryManager.Web.Services.Interfaces;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("cart")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ICacheService cacheService;

        public ShoppingCartController(
            ApplicationDbContext context,
            ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        // GET: /cart
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            string sessionId = this.HttpContext.Session.Id;

            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images.Where(x => x.IsMain == true))
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                cart = new ShoppingCart { SessionId = sessionId };
                this.context.ShoppingCarts.Add(cart);
                await this.context.SaveChangesAsync();
            }

            var config = this.GetImageUrlConfig();
            foreach (var product in cart.Items)
            {
                foreach (var image in product.Product.Images)
                {
                    image.ImageUrl = UrlRewriter.RewriteImageUrl(image.ImageUrl, config.cdnPrefix, config.blobPrefix);
                }
            }

            return this.View(cart);
        }

        // POST: /cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Force session creation.
            string sessionId = this.HttpContext.Session.Id;

            // Load the product to check its stock.
            var product = await this.context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                this.TempData["Error"] = "Product not found.";
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }

            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                cart = new ShoppingCart { SessionId = sessionId };
                this.context.ShoppingCarts.Add(cart);
                await this.context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            int newQuantity = (existingItem?.Quantity ?? 0) + quantity;
            if (newQuantity > product.StockQuantity)
            {
                int availableToAdd = product.StockQuantity - (existingItem?.Quantity ?? 0);
                this.TempData["Error"] = $"Cannot add {quantity} more of {product.Name}. Only {availableToAdd} available in stock.";
                return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    ShoppingCartId = cart.ShoppingCartId
                };
                cart.Items.Add(newItem);
            }

            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
        }

        // POST: /cart/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateItem(int cartItemId, int quantity, string action)
        {
            var item = await this.context.ShoppingCartItems.FirstOrDefaultAsync(i => i.ShoppingCartItemId == cartItemId);
            if (item == null)
            {
                return this.NotFound();
            }

            var product = await this.context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
            if (product == null)
            {
                this.TempData["Error"] = "Product not found.";
                return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
            }

            // Calculate the new quantity based on action.
            int newQuantity = item.Quantity;
            if (action == "increase")
            {
                newQuantity++;
            }
            else if (action == "decrease")
            {
                if (item.Quantity > 1)
                {
                    newQuantity--;
                }
            }

            // Validate new quantity against stock.
            if (newQuantity > product.StockQuantity)
            {
                this.TempData["Error"] = $"Cannot set quantity to {newQuantity} for {product.Name}. Only {product.StockQuantity} available.";
                return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
            }

            item.Quantity = newQuantity;
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
        }

        // POST: /cart/remove
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var item = await this.context.ShoppingCartItems.FirstOrDefaultAsync(i => i.ShoppingCartItemId == cartItemId);
            if (item != null)
            {
                this.context.ShoppingCartItems.Remove(item);
                await this.context.SaveChangesAsync();
            }

            return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
        }

        private (string cdnPrefix, string blobPrefix) GetImageUrlConfig()
        {
            var cdnSetting = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
            var blobSetting = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            return Converters.UrlConverters.ConvertCdnUrls(cdnSetting, blobSetting);
        }
    }
}