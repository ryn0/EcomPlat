using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class BusinessDetails : UserStateInfo
    {
        public int BusinessDetailsId { get; set; }

        /// <summary>
        /// Gets or sets the business name.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the first line of the business address.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the business address.
        /// </summary>
        [MaxLength(100)]
        public string? AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        [MaxLength(50)]
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the state or region.
        /// </summary>
        [MaxLength(50)]
        public string? StateRegion { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the ISO 2-letter country code.
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string CountryIso { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the address type for this business (e.g. Business, Mailing, etc.).
        /// </summary>
        public AddressType AddressType { get; set; }
    }
}
