using EcomPlat.Shipping.Models;

namespace EcomPlat.Shipping.Helpers
{
    public static class ParcelDimensionCalculator
    {

        /// <summary>
        /// Calculates candidate box dimensions for a shopping cart.
        /// It tries every possible grid arrangement and returns the candidate with the smallest difference between width and depth,
        /// and if tied, the smallest overall dimensions.
        /// </summary>
        /// <param name="cart">The shopping cart containing items with product dimensions.</param>
        /// <param name="paddingFactor">Multiplier to add extra space (e.g. 1.20 for 20% extra).</param>
        /// <param name="fixedPadding">Fixed inches to add to each dimension (e.g. 2 inches).</param>
        /// <returns>The calculated box dimensions.</returns>
        public static BoxDimensions CalculateDimensions(ShoppingCart cart, double paddingFactor = 1.20, double fixedPadding = 2.0)
        {
            int totalItems = cart.Items.Sum(item => item.Quantity);
            if (totalItems == 0)
            {
                return new BoxDimensions { Width = 0, Depth = 0, Height = 0 };
            }

            // Use maximum dimensions among items.
            double itemLength = cart.Items.Max(item => (double)item.Product.LengthInches);
            double itemWidth = cart.Items.Max(item => (double)item.Product.WidthInches);
            double itemHeight = cart.Items.Max(item => (double)item.Product.HeightInches);

            BoxDimensions bestCandidate = null;
            double bestDiff = double.MaxValue;
            double bestSum = double.MaxValue;

            // Try every possible number of columns from 1 to totalItems.
            for (int columns = 1; columns <= totalItems; columns++)
            {
                int rows = (int)Math.Ceiling((double)totalItems / columns);

                double candidateWidth = (columns * itemWidth) + fixedPadding;
                double candidateDepth = (rows * itemLength) + fixedPadding;
                double candidateHeight = itemHeight + fixedPadding;

                candidateWidth *= paddingFactor;
                candidateDepth *= paddingFactor;
                candidateHeight *= paddingFactor;

                double sumDimensions = candidateWidth + candidateDepth + candidateHeight;
                double diff = Math.Abs(candidateWidth - candidateDepth);

                if (diff < bestDiff || (Math.Abs(diff - bestDiff) < 0.0001 && sumDimensions < bestSum))
                {
                    bestDiff = diff;
                    bestSum = sumDimensions;
                    bestCandidate = new BoxDimensions
                    {
                        Width = candidateWidth,
                        Depth = candidateDepth,
                        Height = candidateHeight
                    };
                }
            }

            return bestCandidate;
        }
    }
}
