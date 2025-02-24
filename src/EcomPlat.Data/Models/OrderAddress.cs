using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Data.Models
{
    public class OrderAddress
    {
        public int OrderAddressId { get; set; }

        // Foreign key linking to the Order table.
        public int OrderId { get; set; }

        public Order Order { get; set; }

        // Indicates whether this address is Billing or Shipping.
        public AddressType AddressType { get; set; }

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