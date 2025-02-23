// File: Order.cs
using System;
using System.Collections.Generic;

namespace EcomPlat.Data.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }

        // Customer information (expand as needed)
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        // Navigation property for items in the order
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
