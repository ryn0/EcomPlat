using System.Globalization;
using EasyPost;
using EcomPlat.Shipping.Services.Interfaces;
using EasyPost.Models.API;
using EcomPlat.Shipping.Models; // Contains ShippingCostResult

namespace EcomPlat.Shipping.Services.Implementations
{
    /// <summary>
    /// Implementation of IShippingCostCalculator using the EasyPost API.
    /// </summary>
    public class ShippingCostCalculator : IShippingCostCalculator
    {
        private readonly Client client;

        /// <param name="apiKey">Your EasyPost API key.</param>
        public ShippingCostCalculator(string apiKey)
        {
            this.client = new Client(new ClientConfiguration(apiKey));
        }

        /// <summary>
        /// Calculates the shipping cost based on the shopping cart contents and shipment addresses.
        /// This method calculates the total weight and approximate dimensions of the parcel, creates a shipment,
        /// and then selects the USPS Priority rate.
        /// </summary>
        /// <param name="cart">The shopping cart containing items.</param>
        /// <param name="fromAddress">The origin address (EasyPost Address model).</param>
        /// <param name="toAddress">The destination address (EasyPost Address model).</param>
        /// <returns>A ShippingCostResult containing the USPS Priority shipping cost and details.</returns>
        public async Task<ShippingCostResult> CalculateShippingCostAsync(ShoppingCart cart, Address fromAddress, Address toAddress)
        {
            // Calculate the total shipping weight (in ounces).
            double totalWeightOunces = cart.Items.Sum(item => (double)item.Product.ShippingWeightOunces * item.Quantity);

            // Calculate the total volume (in cubic inches) from the cart.
            double totalVolume = cart.Items.Sum(item =>
                (double)(item.Product.LengthInches * item.Product.WidthInches * item.Product.HeightInches) * item.Quantity);

            // Compute an approximate cubic dimension (in inches) based on total volume.
            double dimension = Math.Cbrt(totalVolume);

            // Create a parcel using EasyPost. Wrap the synchronous call in Task.Run.
            Parcel parcel = await Task.Run(() =>
                this.client.Parcel.Create(new EasyPost.Parameters.Parcel.Create
                {
                    Weight = totalWeightOunces,
                    Length = dimension,
                    Width = dimension,
                    Height = dimension
                }));

            // Create a shipment with the provided addresses and the created parcel.
            Shipment shipment = await Task.Run(() =>
                this.client.Shipment.Create(new EasyPost.Parameters.Shipment.Create
                {
                    FromAddress = fromAddress,
                    ToAddress = toAddress,
                    Parcel = parcel
                }));

            // Retrieve the USPS Priority rate from the shipment.
            Rate uspsPriorityRate = shipment.Rates.FirstOrDefault(r =>
                r.Carrier.Equals("USPS", StringComparison.OrdinalIgnoreCase) &&
                r.Service.Equals("Priority", StringComparison.OrdinalIgnoreCase));

            if (uspsPriorityRate == null)
            {
                throw new Exception("USPS Priority rate not available.");
            }

            // Parse the rate value.
            if (!decimal.TryParse(uspsPriorityRate.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cost))
            {
                throw new Exception("Failed to parse shipping cost.");
            }

            return new ShippingCostResult
            {
                Cost = cost,
                Currency = uspsPriorityRate.Currency,
                RateId = uspsPriorityRate.Id
            };
        }
    }
}
