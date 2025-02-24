using EcomPlat.Utilities.KeyGeneration;

namespace EcomPlat.Tests.Utilities.KeyGeneration
{
    public class OrderIdGeneratorTests
    {
        [Fact]
        public void GenerateOrderId_ReturnsStringOfSpecifiedLength()
        {
            // Arrange
            int expectedLength = 8;

            // Act
            string orderId = OrderIdGenerator.GenerateOrderId(expectedLength);

            // Assert
            Assert.NotNull(orderId);
            Assert.Equal(expectedLength, orderId.Length);
        }

        [Fact]
        public void GenerateOrderId_ContainsOnlyAllowedCharacters()
        {
            // Arrange
            int length = 12;
            // This string should match the allowed characters defined in OrderIdGenerator.
            string allowedChars = "34679ACDFGHJKMNPQRTUVWXY";

            // Act
            string orderId = OrderIdGenerator.GenerateOrderId(length);

            // Assert
            foreach (char c in orderId)
            {
                Assert.Contains(c, allowedChars);
            }
        }

        [Fact]
        public void GenerateOrderId_DoesNotContainForbiddenSubstrings()
        {
            // Arrange
            int length = 10;
            string[] forbiddenSubstrings = { "FUCK", "DAMN", "CRAP", "CUNT", "GAY", "FAG" };

            // Act
            string orderId = OrderIdGenerator.GenerateOrderId(length);

            // Assert
            foreach (var forbidden in forbiddenSubstrings)
            {
                Assert.DoesNotContain(forbidden, orderId);
            }
        }

        [Fact]
        public void GenerateOrderId_GeneratesUniqueValues()
        {
            // Arrange
            int length = 8;
            int iterations = 1000;
            var generatedOrderIds = new HashSet<string>();

            // Act & Assert
            for (int i = 0; i < iterations; i++)
            {
                string orderId = OrderIdGenerator.GenerateOrderId(length);
                Assert.DoesNotContain(orderId, generatedOrderIds);
                generatedOrderIds.Add(orderId);
            }
        }
    }
}
