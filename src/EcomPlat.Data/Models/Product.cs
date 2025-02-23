// File: Product.cs
using System.Collections.Generic;

namespace EcomPlat.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public string Sku { get; set; }
        public string UrlKey { get; set; }  // Key to define the product URL
        public int StockQuantity { get; set; }  // Amount currently in stock

        // Foreign keys for category and subcategory
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        // Navigation property for associated images
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
