using System;
using System.Security.Cryptography;
using System.Text;

namespace EcomPlat.Utilities.KeyGeneration
{
    public static class OrderIdGenerator
    {
        /// <summary>
        /// Generates a 16-character numeric order ID string.
        /// </summary>
        /// <returns>A randomly generated 16-character order ID string.</returns>
        public static string GenerateOrderId()
        {
            var orderId = GenerateCandidate(16);
            return orderId;
        }

        private static string GenerateCandidate(int length)
        {
            var orderIdBuilder = new StringBuilder(length);

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int digit = randomBytes[i] % 10; // Generates a digit between 0-9
                    orderIdBuilder.Append(digit);
                }
            }

            return orderIdBuilder.ToString();
        }
    }
}