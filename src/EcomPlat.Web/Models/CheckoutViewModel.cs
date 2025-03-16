using System.Collections.Generic;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcomPlat.Web.Models
{
    public class CheckoutViewModel
    {
        public ShoppingCart Cart { get; set; }
        public IEnumerable<SelectListItem> ShippingOptions { get; set; }
        public ShippingMethod SelectedShippingMethod { get; set; } = ShippingMethod.Standard;
        public decimal ShippingAmount { get; set; } = 10;
        public decimal OrderItemsTotal { get; set; }
        public decimal TotalShippingWeight { get; set; }
        public decimal TotalProductWeight { get; set; }

        public decimal GrandTotal
        {
            get { return this.OrderItemsTotal + this.ShippingAmount; }
        }

        public string Email { get; set; }
        public OrderAddress ShippingAddress { get; set; } = new OrderAddress();

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Crypto;
    }
}
