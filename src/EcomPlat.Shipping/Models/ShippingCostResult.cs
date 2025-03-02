namespace EcomPlat.Shipping.Models
{
    /// <summary>
    /// Represents the shipping cost result.
    /// </summary>
    public class ShippingCostResult
    {
        /// <summary>
        /// The calculated shipping cost.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// The currency of the shipping cost.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The EasyPost rate identifier.
        /// </summary>
        public string RateId { get; set; }
    }
}
