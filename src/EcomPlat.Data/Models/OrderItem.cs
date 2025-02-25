using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    public class OrderItem : UserStateInfo
    {
        public int OrderItemId { get; set; }

        // Quantity of the product ordered
        public int Quantity { get; set; }

        // Capture the price at the time of order
        public decimal UnitPrice { get; set; }

        // Foreign key to the Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Foreign key to the Product
        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }
    }
}