// File: Category.cs
using System.Collections.Generic;

namespace EcomPlat.Data.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UrlKey { get; set; } // Used for product URLs

        // Navigation property to subcategories
        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
