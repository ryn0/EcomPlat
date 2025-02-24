using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class Subcategory : UserStateInfo
    {
        public int SubcategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubcategoryKey { get; set; }

        // Foreign key to the parent category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Navigation property to products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}