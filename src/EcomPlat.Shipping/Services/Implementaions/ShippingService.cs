using System.Globalization;
using EasyPost;
using EasyPost.Models.API;
using EcomPlat.Shipping.Constants;
using EcomPlat.Shipping.Helpers;
using EcomPlat.Shipping.Models;
using EcomPlat.Shipping.Services.Interfaces;

namespace EcomPlat.Shipping.Services.Implementations
{
    /// <summary>
    /// Implementation of IShippingService using the EasyPost API.
    /// </summary>
    public class ShippingService : IShippingService
    {
        private readonly Client client;

        public ShippingService(string apiKey)
        {
            this.client = new Client(new ClientConfiguration(apiKey));
        }

        /// <summary>
        /// Calculates the shipping cost (without purchasing a label) for a given shopping cart.
        /// </summary>
        public async Task<ShippingCostResult> CalculateShippingCostAsync(
            Shipping.Models.ShoppingCart cart,
            EasyPost.Models.API.Address fromAddress,
            EasyPost.Models.API.Address toAddress)
        {
            // Calculate box dimensions and total weight (adding packaging weight, e.g. 2 lbs).
            BoxDimensions dimensions = ParcelDimensionCalculator.CalculateDimensions(cart);
            decimal totalWeight = WeightCalculator.CalculateTotalShippingWeight(cart, 2.0m);

            // Create a shipment and get the USPS Priority rate.
            var (shipment, uspsPriorityRate) = await this.CreateShipmentAndGetUspsPriorityRateAsync(
                (double)totalWeight, dimensions, fromAddress, toAddress);

            // Parse the shipping cost from the rate's Price property.
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

        /// <summary>
        /// Purchases a shipping label for the given shopping cart and returns detailed shipping information.
        /// </summary>
        public async Task<ShippingLabelResult> PurchaseShippingLabelAsync(
            Shipping.Models.ShoppingCart cart,
            EasyPost.Models.API.Address fromAddress,
            EasyPost.Models.API.Address toAddress)
        {
            BoxDimensions dimensions = ParcelDimensionCalculator.CalculateDimensions(cart);
            decimal totalWeight = WeightCalculator.CalculateTotalShippingWeight(cart, 2.0m);

            // Create the shipment and get the USPS Priority rate.
            var (shipment, uspsPriorityRate) = await this.CreateShipmentAndGetUspsPriorityRateAsync(
                (double)totalWeight, dimensions, fromAddress, toAddress);

            // Purchase the shipping label using the USPS Priority rate.
            Shipment purchasedShipment = await Task.Run(() =>
                this.client.Shipment.Buy(shipment.Id, uspsPriorityRate.Id));

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

        /// <summary>
        /// Creates a shipment (by creating a parcel with the given dimensions and weight) and retrieves the USPS Priority rate.
        /// </summary>
        private async Task<(Shipment shipment, Rate uspsPriorityRate)> CreateShipmentAndGetUspsPriorityRateAsync(
            double totalWeight,
            BoxDimensions dimensions,
            EasyPost.Models.API.Address fromAddress,
            EasyPost.Models.API.Address toAddress)
        {
            // Create the parcel using the calculated dimensions and weight.
            Parcel parcel = await Task.Run(() =>
                this.client.Parcel.Create(new EasyPost.Parameters.Parcel.Create
                {
                    Weight = totalWeight,
                    Length = dimensions.Depth,
                    Width = dimensions.Width,
                    Height = dimensions.Height
                }));

            // Create the shipment.
            Shipment shipment = await Task.Run(() =>
                this.client.Shipment.Create(new EasyPost.Parameters.Shipment.Create
                {
                    FromAddress = fromAddress,
                    ToAddress = toAddress,
                    Parcel = parcel
                }));

            // Retrieve the USPS Priority rate.
            Rate uspsPriorityRate = shipment.Rates.FirstOrDefault(r =>
                r.Carrier.Equals(StringConstants.DefaultCarrier, StringComparison.OrdinalIgnoreCase) &&
                r.Service.Equals(StringConstants.DefaultService, StringComparison.OrdinalIgnoreCase));

            if (uspsPriorityRate == null)
            {
                throw new Exception("USPS Priority rate not available.");
            }

            return (shipment, uspsPriorityRate);
        }
    }
}
