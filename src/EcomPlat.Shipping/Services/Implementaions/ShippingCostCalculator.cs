using System.Globalization;
using EasyPost;
using EasyPost.Models.API;
using EcomPlat.Shipping.Models;
using EcomPlat.Shipping.Services.Interfaces;

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
        public async Task<ShippingCostResult> CalculateShippingCostAsync(
     ShoppingCart cart,
     EasyPost.Models.API.Address fromAddress,
     EasyPost.Models.API.Address toAddress)
        {
            // Calculate total weight (in ounces)
            double totalWeightOunces = cart.Items.Sum(item => (double)item.Product.ShippingWeightOunces * item.Quantity);

            // Determine parcel dimensions using the maximum values from all items.
            double maxLength = cart.Items.Max(item => (double)item.Product.LengthInches);
            double maxWidth = cart.Items.Max(item => (double)item.Product.WidthInches);
            double maxHeight = cart.Items.Max(item => (double)item.Product.HeightInches);


            Parcel parcel = await Task.Run(() =>
               this.client.Parcel.Create(new EasyPost.Parameters.Parcel.Create
               {
                   Weight = totalWeightOunces,
                   Length = maxLength,
                   Width = maxWidth,
                   Height = maxHeight
               }));


            // Create a shipment WITHOUT specifying Carrier and Service to avoid conflicting carrier_account_id.
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

            // Parse the rate value (the actual price you'll be charged) from the 'Price' property.
            if (!decimal.TryParse(uspsPriorityRate.Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cost))
            {
                throw new Exception("Failed to parse shipping cost.");
            }

            await this.PurchaseShippingLabelAsync(cart, fromAddress, toAddress);

            return new ShippingCostResult
            {
                Cost = cost,
                Currency = uspsPriorityRate.Currency,
                RateId = uspsPriorityRate.Id
            };
        }


        /// <summary>

        /// <summary>
        /// Purchases a USPS Priority shipping label for the given shopping cart,
        /// using the maximum dimensions among items for the parcel.
        /// Returns a ShippingLabelResult with the cost, label URL, and tracking code.
        /// </summary>
        /// <param name="cart">The shopping cart (from your shipping model) containing items with dimensions and weight.</param>
        /// <param name="fromAddress">The shipment origin address.</param>
        /// <param name="toAddress">The shipment destination address.</param>
        /// <returns>A ShippingLabelResult with shipping details.</returns>
        public async Task<ShippingLabelResult> PurchaseShippingLabelAsync(
       Shipping.Models.ShoppingCart cart,
       EasyPost.Models.API.Address fromAddress,
       EasyPost.Models.API.Address toAddress)
        {
            // Calculate total weight (in ounces)
            double totalWeightOunces = cart.Items.Sum(item => (double)item.Product.ShippingWeightOunces * item.Quantity);

            // Calculate the total volume (in cubic inches) for all items.
            double totalVolume = cart.Items.Sum(item =>
                (double)(item.Product.LengthInches * item.Product.WidthInches * item.Product.HeightInches) * item.Quantity);

            // Add a padding factor to the total volume (e.g., 20% extra space).
            double volumePaddingFactor = 1.20;
            double paddedVolume = totalVolume * volumePaddingFactor;

            // Compute a candidate cube side length from the padded volume.
            double cubeSideCandidate = Math.Cbrt(paddedVolume);

            // Also get the maximum individual dimensions and add a fixed padding (e.g., 2 inches).
            double fixedPadding = 2.0;
            double maxLength = cart.Items.Max(item => (double)item.Product.LengthInches) + fixedPadding;
            double maxWidth = cart.Items.Max(item => (double)item.Product.WidthInches) + fixedPadding;
            double maxHeight = cart.Items.Max(item => (double)item.Product.HeightInches) + fixedPadding;

            // For each dimension, choose the larger: the cube candidate or the padded maximum dimension.
            double finalLength = Math.Max(cubeSideCandidate, maxLength);
            double finalWidth = Math.Max(cubeSideCandidate, maxWidth);
            double finalHeight = Math.Max(cubeSideCandidate, maxHeight);

            // Create a parcel using the calculated weight and dimensions.
            Parcel parcel = await Task.Run(() =>
                this.client.Parcel.Create(new EasyPost.Parameters.Parcel.Create
                {
                    Weight = totalWeightOunces,
                    Length = finalLength,
                    Width = finalWidth,
                    Height = finalHeight
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

            // Purchase the shipping label using the USPS Priority rate.
            Shipment purchasedShipment = await Task.Run(() =>
                this.client.Shipment.Buy(shipment.Id, uspsPriorityRate.Id));

            // Parse the shipping cost from the rate's Price property.
            if (!decimal.TryParse(uspsPriorityRate.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cost))
            {
                throw new Exception("Failed to parse shipping cost.");
            }

            return new ShippingLabelResult
            {
                Cost = cost,
                Currency = uspsPriorityRate.Currency,
                RateId = uspsPriorityRate.Id,
                LabelUrl = purchasedShipment.PostageLabel?.LabelUrl,
                TrackingCode = purchasedShipment.TrackingCode
            };
        }


    }
}