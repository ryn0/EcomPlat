using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
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
            // For anonymous users, use the session ID.
            string sessionId = this.HttpContext.Session.Id;

            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
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
            // Force the session to be created by setting a dummy value.
            this.HttpContext.Session.Set("Init", [1]);

            // Now retrieve the session ID.
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

            // Redirect back to the shopping cart view.
            return this.RedirectToAction("Index", "ShoppingCart", new { area = "Public" });
        }
    }
}
