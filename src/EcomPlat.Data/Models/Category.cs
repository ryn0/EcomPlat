using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    public class Category : UserStateInfo
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [ValidateNever]
        [Required]
        [MaxLength(100)]
        public string CategoryKey { get; set; }

        // Navigation property to subcategories
        public ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    }
}