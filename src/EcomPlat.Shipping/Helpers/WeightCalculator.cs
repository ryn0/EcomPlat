using System.Linq;
 
using EcomPlat.Shipping.Models;

namespace EcomPlat.Shipping.Helpers
{
    public static class WeightCalculator
    {
        /// <summary>
        /// Calculates the total shipping weight for a shopping cart.
        /// This includes the sum of each product's shipping weight (multiplied by its quantity)
        /// plus an additional packaging weight per item.
        /// </summary>
        /// <param name="cart">The shopping cart containing items.</param>
        /// <param name="packagingWeightPerItem">The extra packaging weight per individual item (in ounces).</param>
        /// <returns>Total shipping weight (in ounces).</returns>
        public static decimal CalculateTotalShippingWeight(ShoppingCart cart, decimal packagingWeightPerItem)
        {
            if (cart == null || cart.Items == null)
            {
                return 0m;
            }

            decimal totalProductWeight = cart.Items.Sum(item => item.Product.ShippingWeightOunces * item.Quantity);
            decimal totalPackagingWeight = cart.Items.Sum(item => packagingWeightPerItem * item.Quantity);

            return totalProductWeight + totalPackagingWeight;
        }
    }
}
