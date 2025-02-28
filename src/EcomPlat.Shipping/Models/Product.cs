using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Shipping.Models
{
    /// <summary>
    /// Represents a product in the catalog.
    /// </summary>
    public class Product
    {

        /// <summary>
        /// Gets or sets the shipping weight of the product in ounces.
        /// </summary>
        public decimal ShippingWeightOunces { get; set; }

        /// <summary>
        /// Gets or sets the height of the product in inches.
        /// </summary>
        public decimal HeightInches { get; set; }

        /// <summary>
        /// Gets or sets the width of the product in inches.
        /// </summary>
        public decimal WidthInches { get; set; }

        /// <summary>
        /// Gets or sets the length of the product in inches.
        /// </summary>
        public decimal LengthInches { get; set; }
 
    }
}
