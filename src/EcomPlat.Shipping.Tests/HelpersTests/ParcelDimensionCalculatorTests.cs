using EcomPlat.Shipping.Helpers;
using EcomPlat.Shipping.Models;
 
namespace EcomPlat.Shipping.Tests.HelpersTests
{
 
        public class ParcelDimensionCalculatorTests
        {
            [Fact]
            public void CalculateDimensions_EmptyCart_ReturnsZeroDimensions()
            {
                // Arrange: an empty cart.
                var cart = new ShoppingCart
                {
                    Items = new List<ShoppingCartItem>()
                };

                // Act:
                BoxDimensions dims = ParcelDimensionCalculator.CalculateDimensions(cart);

                // Assert:
                Assert.Equal(0, dims.Width);
                Assert.Equal(0, dims.Depth);
                Assert.Equal(0, dims.Height);
            }

            [Fact]
            public void CalculateDimensions_FiveIdenticalJars_ReturnsExpectedDimensions()
            {
                // Arrange: 5 jars, each 5x5x7 inches.
                var product = new Product
                {
                    LengthInches = 5,
                    WidthInches = 5,
                    HeightInches = 7,
                    ShippingWeightOunces = 10 // arbitrary
                };

                var cart = new ShoppingCart
                {
                    Items = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { Product = product, Quantity = 5 }
                }
                };

                // Act:
                BoxDimensions dims = ParcelDimensionCalculator.CalculateDimensions(cart, paddingFactor: 1.20, fixedPadding: 2.0);

                // For 5 items, one candidate is:
                // columns = 2, rows = 3 (since 2*3 = 6 >= 5)
                // candidateWidth = (2*5 + 2) * 1.20 = (10+2)*1.20 = 12*1.20 = 14.4 inches
                // candidateDepth = (3*5 + 2) * 1.20 = (15+2)*1.20 = 17*1.20 = 20.4 inches
                // candidateHeight = (7 + 2) * 1.20 = 9*1.20 = 10.8 inches

                // Assert:
                Assert.InRange(dims.Width, 14.0, 15.0);
                Assert.InRange(dims.Depth, 20.0, 21.0);
                Assert.InRange(dims.Height, 10.5, 11.5);
            }

            [Fact]
            public void CalculateDimensions_ThreeIdenticalJars_ReturnsExpectedDimensions()
            {
                // Arrange: 3 jars, each 5x5x7 inches.
                var product = new Product
                {
                    LengthInches = 5,
                    WidthInches = 5,
                    HeightInches = 7,
                    ShippingWeightOunces = 10
                };

                var cart = new ShoppingCart
                {
                    Items = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { Product = product, Quantity = 3 }
                }
                };

                // Act:
                BoxDimensions dims = ParcelDimensionCalculator.CalculateDimensions(cart, paddingFactor: 1.20, fixedPadding: 2.0);

                // For 3 items, one candidate is:
                // columns = 2, rows = 2 (since 2*2 = 4 >= 3)
                // candidateWidth = (2*5 + 2) * 1.20 = 12*1.20 = 14.4 inches
                // candidateDepth = (2*5 + 2) * 1.20 = 12*1.20 = 14.4 inches
                // candidateHeight = (7 + 2) * 1.20 = 9*1.20 = 10.8 inches

                // Assert:
                Assert.InRange(dims.Width, 14.0, 15.0);
                Assert.InRange(dims.Depth, 14.0, 15.0);
                Assert.InRange(dims.Height, 10.5, 11.5);
            }
        }
    }
 