using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area("Public")]
    [Route("cart")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /cart
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            string sessionId = this.HttpContext.Session.Id;

            var cart = await this.context.ShoppingCarts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .ThenInclude(i => i.Images)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                cart = new ShoppingCart { SessionId = sessionId };
                this.context.ShoppingCarts.Add(cart);
                await this.context.SaveChangesAsync();
            }

            return this.View(cart);
        }

        // POST: /cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            this.HttpContext.Session.Set("Init", [1]);
            string sessionId = this.HttpContext.Session.Id;

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
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
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

            if (action == "increase")
            {
                item.Quantity++;
            }
            else if (action == "decrease")
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
            }

            await this.context.SaveChangesAsync();
            // Redirect back to the cart.
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
    }
}
