using System.Globalization;
using EasyPost.Models.API;
using EcomPlat.Shipping.Models;
using EcomPlat.Shipping.Services.Interfaces;

namespace EcomPlat.Shipping.Services.Implementaions
{
    /// <summary>
    /// Implementation of IShippingCostCalculator using the EasyPost API.
    /// </summary>
    public class ShippingCostCalculator : IShippingCostCalculator
    {
        private readonly EasyPost.Client client;


        /// <param name="apiKey">Your EasyPost API key.</param>
        public ShippingCostCalculator(string apiKey)
        {
            this.client = new EasyPost.Client(new EasyPost.ClientConfiguration(apiKey));
        }

        /// <summary>
        /// Calculates the shipping cost based on the shopping cart contents and shipment addresses.
        /// </summary>
        /// <param name="cart">The shopping cart containing items (each with a shipping weight in ounces).</param>
        /// <param name="fromAddress">The origin address (an EasyPost Address model).</param>
        /// <param name="toAddress">The destination address (an EasyPost Address model).</param>
        /// <returns>A ShippingCostResult containing the calculated shipping cost and details.</returns>
        public async Task<ShippingCostResult> CalculateShippingCostAsync(ShoppingCart cart, Address fromAddress, Address toAddress)
        {
            // Calculate the total shipping weight (in ounces).
            double totalWeightOunces = cart.Items.Sum(item => (double)item.Product.ShippingWeightOunces * item.Quantity);

            // Calculate the total volume (in cubic inches) from the cart.
            double totalVolume = cart.Items.Sum(item =>
                (double)(item.Product.LengthInches * item.Product.WidthInches * item.Product.HeightInches) * item.Quantity);

            // Compute an approximate cubic dimension (in inches) based on total volume.
            double dimension = Math.Cbrt(totalVolume);

            // Create a parcel using EasyPost. Wrap synchronous call in Task.Run.
            Parcel parcel = await Task.Run(() =>
                           this.client.Parcel.Create(new EasyPost.Parameters.Parcel.Create
                           {
                               Weight = totalWeightOunces,
                               // Remove PredefinedPackage to use custom dimensions.
                               Length = dimension,
                               Width = dimension,
                               Height = dimension
                           }));

            // Create a shipment with the given addresses and the created parcel.
            Shipment shipment = await Task.Run(() =>
                this.client.Shipment.Create(new EasyPost.Parameters.Shipment.Create
                {
                    FromAddress = fromAddress,
                    ToAddress = toAddress,
                    Parcel = parcel
                }));

            // Get the lowest rate from the shipment.
            Rate lowestRate = shipment.LowestRate();

            // Parse the rate value.
            if (!decimal.TryParse(lowestRate.ListRate, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cost))
            {
                throw new Exception("Failed to parse shipping cost.");
            }

            // Return the result.
            return new ShippingCostResult
            {
                Cost = cost,
                Currency = lowestRate.Currency,
                RateId = lowestRate.Id
            };
        }
    }
}