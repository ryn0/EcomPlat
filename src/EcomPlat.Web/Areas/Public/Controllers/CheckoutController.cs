﻿using System;
using System.Linq;
using System.Threading.Tasks;
using EcomPlat.Data.Constants;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
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
            // Ensure the session is created.
            this.HttpContext.Session.Set("Init", new byte[] { 1 });
            string sessionId = this.HttpContext.Session.Id;

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

            return this.View(viewModel);
        }

        // POST: /checkout
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // Reload the cart to ensure up-to-date product stock levels.
            string sessionId = this.HttpContext.Session.Id;
            model.Cart = await this.context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            // Validate each cart item against product stock.
            foreach (var item in model.Cart.Items)
            {
                var product = await this.context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                if (product != null && item.Quantity > product.StockQuantity)
                {
                    this.ModelState.AddModelError("",
                        $"The quantity for {product.Name} exceeds available stock. Available: {product.StockQuantity}");
                }
            }

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
                    })
                    .ToList();
                return this.View(model);
            }

            // TODO: Process the order (create Order record, process payment, etc.)

            return this.RedirectToAction("OrderConfirmation", "Checkout", new { area = "Public" });
        }

        /// <summary>
        /// Calculates shipping charges based on the shipping method and order total.
        /// </summary>
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
