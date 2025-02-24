using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class Supplier : UserStateInfo
    {
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string? AddressLine1 { get; set; }

        [MaxLength(100)]
        public string? AddressLine2 { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        /// <summary>
        /// The state or region.
        /// </summary>
        [MaxLength(50)]
        public string? StateRegion { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [MaxLength(2)]
        public string CountryIso { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        // Navigation property: A supplier can have many product inventory records.
        public ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
    }
}