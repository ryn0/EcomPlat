using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class Category : UserStateInfo
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryKey { get; set; }

        // Navigation property to subcategories
        public ICollection<Subcategory> SubCategories { get; set; } = new List<Subcategory>();
    }
}