using System.Security.Cryptography;

namespace EcomPlat.Utilities.KeyGeneration
{
    public static class OrderIdGenerator
    {
        private static readonly char[] AllowedChars = "34679ACDFGHJKMNPQRTUVWXY".ToCharArray();

        // List of substrings that are not allowed in the generated order ID.
        private static readonly string[] ForbiddenSubstrings =
        [
            "FUCK", "DAMN", "CRAP", "CUNT", "GAY", "FAG"
        ];

        /// <summary>
        /// Generates a non-sequential, customer-friendly order ID that avoids forbidden substrings.
        /// </summary>
        /// <param name="length">The length of the order ID string (default is 8).</param>
        /// <returns>A randomly generated order ID string.</returns>
        public static string GenerateOrderId(int length = 8)
        {
            string orderId;
            do
            {
                orderId = GenerateCandidate(length);
            }
            while (ContainsForbiddenSubstring(orderId));

            return orderId;
        }

        private static string GenerateCandidate(int length)
        {
            var orderIdChars = new char[length];

            // Use a cryptographically secure random generator.
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % AllowedChars.Length;
                    orderIdChars[i] = AllowedChars[index];
                }
            }

            return new string(orderIdChars);
        }

        private static bool ContainsForbiddenSubstring(string candidate)
        {
            foreach (var forbidden in ForbiddenSubstrings)
            {
                if (candidate.Contains(forbidden))
                {
                    return true;
                }
            }
            return false;
        }
    }
}