using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext context;

        public CheckoutController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: /checkout
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Use session to identify the cart
            string sessionId = this.HttpContext.Session.Id;

            // Load the cart (include products for order summary)
            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            // If no cart exists or it's empty, redirect to products page.
            if (cart == null || !cart.Items.Any())
            {
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }

            // Calculate the order total
            decimal orderTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            // Create shipping options from the ShippingMethod enum.
            var shippingOptions = Enum.GetValues(typeof(ShippingMethod))
                .Cast<ShippingMethod>()
                .Select(sm => new SelectListItem
                {
                    Value = sm.ToString(),
                    Text = sm.ToString()
                }).ToList();

            // Set a default shipping method (for example, Standard) and shipping amount.
            ShippingMethod defaultShipping = ShippingMethod.Standard;
            decimal shippingAmount = this.CalculateShippingCharge(defaultShipping, orderTotal);

            // Build the view model.
            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                OrderTotal = orderTotal,
                ShippingOptions = shippingOptions,
                SelectedShippingMethod = defaultShipping,
                ShippingAmount = shippingAmount
            };

            return this.View(viewModel);
        }

        // POST: /checkout
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                // Rebuild shipping options if there's an error.
                model.ShippingOptions = Enum.GetValues(typeof(ShippingMethod))
                    .Cast<ShippingMethod>()
                    .Select(sm => new SelectListItem
                    {
                        Value = sm.ToString(),
                        Text = sm.ToString(),
                        Selected = sm == model.SelectedShippingMethod
                    }).ToList();
                return this.View(model);
            }

            // TODO: Create an Order from the ShoppingCart, set shipping info, process payment, etc.
            // For now, simply redirect to a confirmation page (or home page).

            // Clear the shopping cart after order is placed (if needed)
            // Example:
            // var cart = await this.context.ShoppingCarts.FirstOrDefaultAsync(c => c.SessionId == this.HttpContext.Session.Id);
            // this.context.ShoppingCarts.Remove(cart);
            // await this.context.SaveChangesAsync();

            return this.RedirectToAction("OrderConfirmation", "Checkout", new { area = "Public" });
        }

        // Helper method to calculate shipping charges based on the shipping method and order total.
        private decimal CalculateShippingCharge(ShippingMethod shippingMethod, decimal orderTotal)
        {
            // For demonstration, simple logic:
            switch (shippingMethod)
            {
                case ShippingMethod.Express:
                    return 15.00m;
                case ShippingMethod.Overnight:
                    return 25.00m;
                case ShippingMethod.Standard:
                default:
                    return 5.00m;
            }
        }
    }
}
