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
        public ShippingMethod SelectedShippingMethod { get; set; }
        public decimal ShippingAmount { get; set; } = 10;
        public decimal OrderTotal { get; set; }
        public OrderAddress ShippingAddress { get; set; } = new OrderAddress();
    }
}
