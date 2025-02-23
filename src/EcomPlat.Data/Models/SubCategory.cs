// File: SubCategory.cs
using System.Collections.Generic;

namespace EcomPlat.Data.Models
{
    public class SubCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UrlKey { get; set; }  // Used for product URLs

        // Foreign key to the parent category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Navigation property to products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
