// File: OrderItem.cs
namespace EcomPlat.Data.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Quantity of the product ordered
        public int Quantity { get; set; }

        // Capture the price at the time of order
        public decimal UnitPrice { get; set; }

        // Foreign key to the Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Foreign key to the Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
