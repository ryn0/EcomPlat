using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Shipping.Services.Interfaces;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcomPlat.Web.Areas.Account.Controllers
{
    [Area(StringConstants.AccountArea)]
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IShippingService shippingService;

        public OrderManagementController(ApplicationDbContext context, IShippingService shippingService)
        {
            this.context = context;
            this.shippingService = shippingService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await this.context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return this.View(orders);
        }

        // GET: Admin/Orders/Details/{orderId}
        public async Task<IActionResult> Details(int orderId)
        {
            var order = await this.context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Addresses)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return this.NotFound();
            }

            int pounds = (int)(order.ShippingWeightOunces / 16);
            int ounces = (int)(order.ShippingWeightOunces % 16);

            this.ViewBag.PoundList = Enumerable.Range(0, 50).Select(i =>
                new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = i == pounds
                }).ToList();

            this.ViewBag.OunceList = Enumerable.Range(0, 16).Select(i =>
                new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = i == ounces
                }).ToList();

            return this.View(order);
        }

        // POST: Admin/Orders/BuyLabel/{orderId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyLabel(int orderId)
        {
            var order = await this.context.Orders
                .Include(o => o.Addresses)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Ensure Product is included
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || !order.Addresses.Any())
            {
                return this.NotFound();
            }

            var fromAddress = await this.GetBusinessAddress();

            var orderShippingAddress = order.Addresses
                                            .Where(x => x.AddressType == Data.Models.AddressType.Shipping)
                                            .First();

            var toAddress = new EasyPost.Models.API.Address
            {
                Street1 = orderShippingAddress.AddressLine1,
                Street2 = orderShippingAddress.AddressLine2,
                City = orderShippingAddress.City,
                State = orderShippingAddress.StateRegion,
                Zip = orderShippingAddress.PostalCode,
                Country = orderShippingAddress.CountryIso
            };

            // todo: remove this and use the actual weight of the package and the actual package size
            var cart = Converters.OrderToCartConverter.ConvertToCart(order);
            var labelResult = await this.shippingService.PurchaseShippingLabelAsync(cart, fromAddress, toAddress);

            // Update order with tracking details
            order.ShippingCarrier = ShippingCarrier.USPS;
            order.ShipmentTrackingId = labelResult?.TrackingCode;
            order.ShippingStatus = ShippingStatus.LabelCreated;
            order.ShippingLabelUrl = labelResult?.LabelUrl;
            this.context.Orders.Update(order);
            await this.context.SaveChangesAsync();

            return this.RedirectToAction("Details", new { orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFinalWeight(int orderId, int pounds, int ounces)
        {
            var order = await this.context.Orders
                                          .Include(o => o.OrderItems)
                                          .ThenInclude(p => p.Product)
                                          .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return this.NotFound();
            }

            var cart = Converters.OrderToCartConverter.ConvertToCart(order);
            decimal finalWeight = (pounds * 16) + ounces;

            if (finalWeight < cart.TotalProductShippingWeight)
            {
                this.TempData["Error"] = $"Final weight must be greater than or equal to calculated weight ({order.ShippingWeightOunces:F2} oz)";
                return this.RedirectToAction("Details", new { orderId });
            }

            order.ShippingWeightOunces = finalWeight;
            await this.context.SaveChangesAsync();

            this.TempData["Success"] = "Final shipping weight updated.";
            return this.RedirectToAction("Details", new { orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsShipped(int orderId)
        {
            var order = await this.context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return this.NotFound();
            }

            if (!string.IsNullOrEmpty(order.ShippingLabelUrl) && order.ShippingStatus == ShippingStatus.Pending)
            {
                order.ShippingStatus = ShippingStatus.Shipped;
                await this.context.SaveChangesAsync();
            }

            return this.RedirectToAction("Details", new { orderId });
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