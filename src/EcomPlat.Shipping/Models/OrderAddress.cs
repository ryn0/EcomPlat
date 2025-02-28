using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Shipping.Models
{
    public class OrderAddress
    {
         

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Address fields
        [Required]
        [MaxLength(100)]
        public string AddressLine1 { get; set; }

        [MaxLength(100)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        /// <summary>
        /// The state or region.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string StateRegion { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(2)]
        public string CountryIso { get; set; }
    }
}