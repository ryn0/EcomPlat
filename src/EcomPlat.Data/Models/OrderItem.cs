using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    public class OrderItem : UserStateInfo
    {
        public int OrderItemId { get; set; }

        /// <summary>
        /// Quantity of the product ordered.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Capture the price at the time of order.
        /// </summary>
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Foreign key to the Order
        /// </summary>
        public int OrderId { get; set; }
        public Order Order { get; set; }

        /// <summary>
        /// Foreign key to the Product.
        /// </summary>
        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }
    }
}