using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Data.Repositories.Interfaces;
using EcomPlat.Shipping.Models;
using EcomPlat.Shipping.Services.Interfaces;
using EcomPlat.Shipping.Validation;
using EcomPlat.Utilities.KeyGeneration;
using EcomPlat.Web.Constants;
using EcomPlat.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NowPayments.API.Interfaces;
using NowPayments.API.Models;

namespace EcomPlat.Web.Areas.Public.Controllers
{
    [Area(Constants.StringConstants.PublicArea)]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private const decimal ShippingChargeUSD = 5;
        private readonly ApplicationDbContext context;
        private readonly IShippingService shippingService;
        private readonly IOrderRepository orderRepository;
        private readonly INowPaymentsService paymentService;

        public CheckoutController(
            ApplicationDbContext context,
            IShippingService shippingService,
            IOrderRepository orderRepository,
            INowPaymentsService paymentService)
        {
            this.context = context;
            this.shippingService = shippingService;
            this.orderRepository = orderRepository;
            this.paymentService = paymentService;
        }


        // POST: /checkout
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutModel checkoutModel)
        {
            this.HttpContext.Session.Set("Init", new byte[] { 1 });
            string sessionId = this.HttpContext.Session.Id;
            Data.Models.ShoppingCart? cart = await this.GetCart(sessionId);

            if (cart == null || !cart.Items.Any())
            {
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }

            // Validate stock
            foreach (var item in cart.Items)
            {
                var product = await this.context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                if (product != null && item.Quantity > product.StockQuantity)
                {
                    this.ModelState.AddModelError("", $"The quantity for {product.Name} exceeds available stock. Available: {product.StockQuantity}");
                }
            }

            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            // Retrieve shipping address
            var shippingAddressJson = this.TempData.Peek("ShippingAddress") as string;
            if (string.IsNullOrEmpty(shippingAddressJson))
            {
                return this.RedirectToAction("ShippingAddress");
            }

            var shippingAddress = JsonConvert.DeserializeObject<OrderAddress>(shippingAddressJson);
            CheckoutViewModel checkoutViewModel = BuildCheckoutModel(cart);
            Order order = BuildOrder(checkoutViewModel, checkoutModel, cart, shippingAddress);

            while (await this.orderRepository.FindByCustomerOrderIdAsync(order.CustomerOrderId) != null)
            {
                order.CustomerOrderId = OrderIdGenerator.GenerateOrderId();
            }

            // Save order to database
            await this.orderRepository.CreateAsync(order);

            // Clear the shopping cart after successful order creation
            this.context.ShoppingCarts.Remove(cart);
            await this.context.SaveChangesAsync();

            // Handle Crypto Payment Redirect
            var paymentRequest = new PaymentRequest
            {
                OrderId = order.CustomerOrderId,
                PriceAmount = order.OrderTotal,
                PriceCurrency = Currency.USD.ToString(),
                IsFeePaidByUser = true,
                OrderDescription = $"Order: {order.CustomerOrderId}",
                PayCurrency = Currency.XMR.ToString()
            };

            this.paymentService.SetDefaultUrls(paymentRequest);

            var invoiceFromProcessor = await this.paymentService.CreateInvoice(paymentRequest);

            order.ProcessorInvoiceId = invoiceFromProcessor?.Id;
            order.PaymentResponse = invoiceFromProcessor.InvoiceUrl;

            await this.orderRepository.UpdateAsync(order);

            return this.Redirect(invoiceFromProcessor.InvoiceUrl);
        }

        // GET: /checkout
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Ensure session exists.
            this.HttpContext.Session.Set("Init", new byte[] { 1 });
            string sessionId = this.HttpContext.Session.Id;

            Data.Models.ShoppingCart? cart = await this.GetCart(sessionId);

            if (cart == null || !cart.Items.Any())
            {
                return this.RedirectToAction("Index", "Products", new { area = "Public" });
            }

            CheckoutViewModel viewModel = BuildCheckoutModel(cart);

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

            // Retrieve the business shipping address from BusinessDetails.
            var fromAddress = await this.GetBusinessAddress();

            var toAddress = new EasyPost.Models.API.Address()
            {
                City = viewModel.ShippingAddress.City,
                Country = viewModel.ShippingAddress.CountryIso,
                Name = viewModel.ShippingAddress.Name,
                State = viewModel.ShippingAddress.StateRegion,
                Street1 = viewModel.ShippingAddress.AddressLine1,
                Street2 = viewModel.ShippingAddress.AddressLine2,
                Zip = viewModel.ShippingAddress.PostalCode
            };

            // Get the shipping cost using the business (fromAddress) and customer (toAddress) addresses.
            var shippingCostResult = await this.GetShippingCostAsync(viewModel, fromAddress, toAddress);
            var shippingTotal = shippingCostResult.Cost + ShippingChargeUSD;
            viewModel.ShippingAmount = shippingTotal;

            return this.View(viewModel);
        }

        [HttpGet("canceled")]
        public async Task<IActionResult> Canceled()
        {
            return this.View();
        }

        [HttpGet("checkout/partiallypaid")]
        public async Task<IActionResult> PartiallyPaid()
        {
            return this.View();
        }

        [HttpGet("nowpaymentssuccess")]
        [AllowAnonymous]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
               "StyleCop.CSharp.NamingRules",
               "SA1313:Parameter names should begin with lower-case letter",
               Justification = "This is the param from them")]
        public async Task<IActionResult> NowPaymentsSuccess([FromQuery] string NP_id)
        {
            var processorInvoice = await this.paymentService.GetPaymentStatusAsync(NP_id);

            if (processorInvoice == null)
            {
                return this.BadRequest(new { Error = StringConstants.InvoiceNotFound });
            }

            if (processorInvoice.OrderId == null)
            {
                return this.BadRequest(new { Error = "Order ID not found." });
            }

            var order = await this.orderRepository.FindByCustomerOrderIdAsync(processorInvoice.OrderId);

            if (order == null)
            {
                return this.BadRequest(new { Error = StringConstants.InvoiceNotFound });
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentResponse = NP_id;

            var viewModel = new SuccessViewModel
            {
                CustomerOrderId = order.CustomerOrderId
            };

            return this.View("Success", viewModel);
        }

        private async Task<Data.Models.ShoppingCart?> GetCart(string sessionId)
        {
            // Load the shopping cart.
            return await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        private static CheckoutViewModel BuildCheckoutModel(Data.Models.ShoppingCart? cart)
        {
            // Calculate order total.
            decimal orderTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
            // Calculate total shipping weight (in ounces).
            decimal totalShippingWeight = cart.Items.Sum(i => i.Product.ShippingWeightOunces * i.Quantity);
            decimal totalProductWeight = cart.Items.Sum(i => i.Product.ProductWeightOunces * i.Quantity);

            var shippingOptions = Enum.GetValues(typeof(ShippingMethod))
                .Cast<ShippingMethod>()
                .Select(sm => new SelectListItem
                {
                    Value = sm.ToString(),
                    Text = sm.ToString()
                })
                .ToList();

            ShippingMethod defaultShipping = ShippingMethod.Standard;

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                OrderItemsTotal = orderTotal,
                TotalShippingWeight = totalShippingWeight,
                TotalProductWeight = totalProductWeight,
                ShippingOptions = shippingOptions,
                SelectedShippingMethod = defaultShipping,
                ShippingAddress = new OrderAddress() // default new address
            };
            return viewModel;
        }

        // GET: /checkout/order-confirmation/{orderId}
        [HttpGet("order-confirmation/{orderId}")]
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var order = await this.orderRepository.FindByCustomerOrderIdAsync(orderId);
            if (order == null)
            {
                return this.NotFound();
            }

            return this.View(order);
        }

        // GET: /checkout/shipping-address
        [HttpGet("shipping-address")]
        public IActionResult ShippingAddress()
        {
            // If a shipping address is already set, redirect to checkout.
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
            // Remove ModelState errors for fields not needed.
            this.ModelState.Remove("Order");
            this.ModelState.Remove("OrderId");
            this.ModelState.Remove("CountryIso");

            // Retrieve the EasyPost API key.
            var apiKeySetting = await this.context.ConfigSettings
                .FirstOrDefaultAsync(cs => cs.SiteConfigSetting == SiteConfigSetting.EasyPostApiKey);

            if (apiKeySetting == null || string.IsNullOrEmpty(apiKeySetting.Content))
            {
                this.ModelState.AddModelError("", "Shipping configuration error.");
            }
            else
            {
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
                    var errorMsg = string.IsNullOrWhiteSpace(validationResult.ErrorMessage)
                        ? "Address validation failed."
                        : "Address validation failed: " + validationResult.ErrorMessage;
                    this.ModelState.AddModelError("", errorMsg);
                }
                else
                {
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

            this.TempData["ShippingAddress"] = JsonConvert.SerializeObject(address);
            return this.RedirectToAction("Index");
        }

        // GET: /checkout/edit-address
        [HttpGet("edit-address")]
        public IActionResult EditShippingAddress()
        {
            this.TempData.Remove("ShippingAddress");
            return this.RedirectToAction("ShippingAddress");
        }

        private static Order BuildOrder(
            CheckoutViewModel checkoutViewModel,
            CheckoutModel checkoutModel,
            Data.Models.ShoppingCart? cart,
            OrderAddress? shippingAddress)
        {
            // Create the Order object
            var order = new Order
            {
                CustomerOrderId = OrderIdGenerator.GenerateOrderId(),
                OrderDate = DateTime.UtcNow,
                CustomerEmail = checkoutModel.Email,
                SalePrice = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                BillingCurrency = Currency.USD,
                PaymentCurrency = Currency.Unknown,
                PaymentMethod = checkoutViewModel.PaymentMethod, // Change dynamically based on selection.
                PaymentProcessor = PaymentProcessor.NowPayments, // Change dynamically.
                ShippingWeightOunces = cart.Items.Sum(i => i.Product.ShippingWeightOunces * i.Quantity),
                ShippingAmount = checkoutViewModel.ShippingAmount,
                OrderTotal = checkoutViewModel.GrandTotal,
                ShippingMethod = checkoutViewModel.SelectedShippingMethod,
                PaymentStatus = PaymentStatus.Pending,
                ShippingStatus = ShippingStatus.Pending,
                ShippingCarrier = ShippingCarrier.USPS, // Change based on the selected carrier.
                ShipmentTrackingId = string.Empty,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.Product.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price,
                    TotalPrice = i.Product.Price * i.Quantity
                }).ToList(),
                Addresses = new List<OrderAddress> { shippingAddress }
            };
            return order;
        }
 
        private async Task<ShippingCostResult> GetShippingCostAsync(
            CheckoutViewModel viewModel,
            EasyPost.Models.API.Address fromAddress,
            EasyPost.Models.API.Address toAddress)
        {
            var shippingCart = new Shipping.Models.ShoppingCart();
            foreach (var item in viewModel.Cart.Items)
            {
                shippingCart.Items.Add(new Shipping.Models.ShoppingCartItem()
                {
                    Product = new Shipping.Models.Product()
                    {
                        HeightInches = item.Product.HeightInches,
                        LengthInches = item.Product.LengthInches,
                        ShippingWeightOunces = item.Product.ShippingWeightOunces,
                        WidthInches = item.Product.WidthInches
                    },
                    Quantity = item.Quantity
                });
            }

            return await this.shippingService.CalculateShippingCostAsync(shippingCart, fromAddress, toAddress);
        }

        private async Task<EasyPost.Models.API.Address> GetBusinessAddress()
        {
            var businessDetails = await this.context.BusinessDetails.FirstOrDefaultAsync()
                ?? throw new Exception("Business shipping address is not configured.");

            var fromAddress = new EasyPost.Models.API.Address()
            {
                City = businessDetails.City,
                Country = businessDetails.CountryIso,
                Name = businessDetails.Name,
                State = businessDetails.StateRegion,
                Street1 = businessDetails.AddressLine1,
                Street2 = businessDetails.AddressLine2,
                Zip = businessDetails.PostalCode
            };
            return fromAddress;
        }
    }
}