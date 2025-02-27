using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Shipping.Validation;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        // GET: /checkout/shipping-address
        [HttpGet("shipping-address")]
        public IActionResult ShippingAddress()
        {
            // If a shipping address has already been set, redirect to checkout.
            var shippingAddressJson = this.TempData.Peek("ShippingAddress") as string;
            if (!string.IsNullOrEmpty(shippingAddressJson))
            {
                return this.RedirectToAction("Index");
            }
            return this.View();
        }

        // POST: /checkout/shipping-address
        [HttpPost("shipping-address")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShippingAddress(OrderAddress address)
        {
            // Remove any ModelState errors for properties not needed in this context.
            this.ModelState.Remove("Order");
            this.ModelState.Remove("OrderId");
            this.ModelState.Remove("CountryIso");

            // Retrieve the EasyPost API key from configuration.
            var apiKeySetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.EasyPostApiKey);

            if (apiKeySetting == null || string.IsNullOrEmpty(apiKeySetting.Content))
            {
                this.ModelState.AddModelError("", "Shipping configuration error.");
            }
            else
            {
                // Initialize EasyPost.
                AddressValidator.Initialize(apiKeySetting.Content);
                var validationResult = await AddressValidator.ValidateAddressAsync(
                    address.AddressLine1,
                    address.AddressLine2,
                    address.City,
                    address.StateRegion,
                    address.PostalCode,
                    address.CountryIso);

                if (!validationResult.IsValid)
                {
                    // If no error message is provided, use a default one.
                    var errorMsg = string.IsNullOrWhiteSpace(validationResult.ErrorMessage)
                        ? "Address validation failed."
                        : "Address validation failed: " + validationResult.ErrorMessage;
                    this.ModelState.AddModelError("", errorMsg);
                }
                else
                {
                    // Use the verified address values.
                    address.AddressLine1 = validationResult.VerifiedAddress.Street1;
                    address.AddressLine2 = validationResult.VerifiedAddress.Street2;
                    address.City = validationResult.VerifiedAddress.City;
                    address.StateRegion = validationResult.VerifiedAddress.State;
                    address.PostalCode = validationResult.VerifiedAddress.Zip;
                    address.CountryIso = validationResult.VerifiedAddress.Country;
                    address.AddressType = AddressType.Shipping;
                }
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(address);
            }

            // Save the validated shipping address in TempData.
            this.TempData["ShippingAddress"] = JsonConvert.SerializeObject(address);
            return this.RedirectToAction("Index");
        }


        // GET: /checkout
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Ensure session exists.
            this.HttpContext.Session.Set("Init", new byte[] { 1 });
            string sessionId = this.HttpContext.Session.Id;

            // Load the shopping cart.
            var cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            if (cart == null || !cart.Items.Any())
            {
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }
            decimal orderTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            var shippingOptions = Enum.GetValues(typeof(ShippingMethod))
                .Cast<ShippingMethod>()
                .Select(sm => new SelectListItem
                {
                    Value = sm.ToString(),
                    Text = sm.ToString()
                })
                .ToList();
            ShippingMethod defaultShipping = ShippingMethod.Standard;
            decimal shippingAmount = this.CalculateShippingCharge(defaultShipping, orderTotal);

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                OrderTotal = orderTotal,
                ShippingOptions = shippingOptions,
                SelectedShippingMethod = defaultShipping,
                ShippingAmount = shippingAmount
            };

            // Load shipping address from TempData.
            var shippingAddressJson = this.TempData.Peek("ShippingAddress") as string;
            if (!string.IsNullOrEmpty(shippingAddressJson))
            {
                viewModel.ShippingAddress = JsonConvert.DeserializeObject<OrderAddress>(shippingAddressJson);
            }
            else
            {
                // If no shipping address is set, redirect to shipping address input.
                return this.RedirectToAction("ShippingAddress");
            }

            return this.View(viewModel);
        }

        // GET: /checkout/edit-address
        [HttpGet("edit-address")]
        public IActionResult EditShippingAddress()
        {
            // Remove the saved address so the user can re-enter.
            this.TempData.Remove("ShippingAddress");
            return this.RedirectToAction("ShippingAddress");
        }

        // POST: /checkout
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            string sessionId = this.HttpContext.Session.Id;
            model.Cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            // Validate each cart item against stock.
            foreach (var item in model.Cart.Items)
            {
                var product = await this.context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                if (product != null && item.Quantity > product.StockQuantity)
                {
                    this.ModelState.AddModelError("", $"The quantity for {product.Name} exceeds available stock. Available: {product.StockQuantity}");
                }
            }

            if (!this.ModelState.IsValid)
            {
                model.ShippingOptions = Enum.GetValues(typeof(ShippingMethod))
                    .Cast<ShippingMethod>()
                    .Select(sm => new SelectListItem
                    {
                        Value = sm.ToString(),
                        Text = sm.ToString(),
                        Selected = sm == model.SelectedShippingMethod
                    })
                    .ToList();
                return this.View(model);
            }

            // TODO: Process the order (create order record, process payment, etc.)

            return this.RedirectToAction("OrderConfirmation", "Checkout", new { area = "Public" });
        }

        private decimal CalculateShippingCharge(ShippingMethod shippingMethod, decimal orderTotal)
        {
            return shippingMethod switch
            {
                ShippingMethod.Express => 15.00m,
                ShippingMethod.Overnight => 25.00m,
                ShippingMethod.Standard => 5.00m,
                _ => 5.00m,
            };
        }
    }
}
