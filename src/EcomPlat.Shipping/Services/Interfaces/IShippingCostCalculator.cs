using EasyPost.Models.API;
using EcomPlat.Shipping.Models;

namespace EcomPlat.Shipping.Services.Interfaces

{
    /// <summary>
    /// Calculates shipping costs using EasyPost API.
    /// </summary>
    public interface IShippingCostCalculator
    {
        /// <summary>
        /// Calculates the shipping cost based on the contents of the shopping cart and the shipment addresses.
        /// </summary>
        /// <param name="cart">The shopping cart with its items.</param>
        /// <param name="fromAddress">The shipment’s origin address.</param>
        /// <param name="toAddress">The shipment’s destination address.</param>
        /// <returns>A ShippingCostResult containing the calculated shipping cost and details.</returns>
        Task<ShippingCostResult> CalculateShippingCostAsync(ShoppingCart cart, Address fromAddress, Address toAddress);
    }
}
