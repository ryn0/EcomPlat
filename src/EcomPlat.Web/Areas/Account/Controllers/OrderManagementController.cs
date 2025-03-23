using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Shipping.Services.Interfaces;
using EcomPlat.Web.Constants;
using Microsoft.AspNetCore.Mvc;
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

            var toAddress = new EasyPost.Models.API.Address
            {
                Street1 = order.Addresses.First().AddressLine1,
                City = order.Addresses.First().City,
                State = order.Addresses.First().StateRegion,
                Zip = order.Addresses.First().PostalCode,
                Country = order.Addresses.First().CountryIso
            };

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