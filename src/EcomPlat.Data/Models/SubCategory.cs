using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    public class Subcategory : UserStateInfo
    {
        public int SubcategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [ValidateNever]
        [Required]
        [MaxLength(100)]
        public string SubcategoryKey { get; set; }

        // Foreign key to the parent category
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }

        // Navigation property to products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}