using EcomPlat.Utilities.KeyGeneration;
using Xunit;

namespace EcomPlat.Tests.Utilities.KeyGeneration
{
    public class OrderIdGeneratorTests
    {
        [Fact]
        public void GenerateOrderId_ReturnsStringOfFixed16Digits()
        {
            // Act
            string orderId = OrderIdGenerator.GenerateOrderId();

            // Assert
            Assert.NotNull(orderId);
            Assert.Equal(16, orderId.Length);
            Assert.All(orderId, c => Assert.InRange(c, '0', '9')); // Ensure all characters are digits (0-9)
        }

        [Fact]
        public void GenerateOrderId_ContainsOnlyDigits()
        {
            // Act
            string orderId = OrderIdGenerator.GenerateOrderId();

            // Assert
            Assert.All(orderId, c => Assert.True(char.IsDigit(c))); // Ensure each character is a digit
        }

        [Fact]
        public void GenerateOrderId_AllowsLeadingZeros()
        {
            // Act
            bool foundLeadingZero = false;

            for (int i = 0; i < 1000; i++)
            {
                string orderId = OrderIdGenerator.GenerateOrderId();
                if (orderId.StartsWith("0"))
                {
                    foundLeadingZero = true;
                    break;
                }
            }

            // Assert
            Assert.True(foundLeadingZero, "At least one generated order ID should start with a 0");
        }

        [Fact]
        public void GenerateOrderId_GeneratesUniqueValues()
        {
            // Arrange
            int iterations = 1000;
            var generatedOrderIds = new HashSet<string>();

            // Act & Assert
            for (int i = 0; i < iterations; i++)
            {
                string orderId = OrderIdGenerator.GenerateOrderId();
                Assert.DoesNotContain(orderId, generatedOrderIds);
                generatedOrderIds.Add(orderId);
            }
        }
    }
}
