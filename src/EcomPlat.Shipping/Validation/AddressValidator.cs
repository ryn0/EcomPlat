using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyPost;
using EasyPost.Models;
using EasyPost.Models.API;
using EcomPlat.Shipping.Models;


namespace EcomPlat.Shipping.Validation
{
    public static class AddressValidator
    {
        private static Client client;

        /// <summary>
        /// Initializes the EasyPost client with the provided API key.
        /// </summary>
        /// <param name="apiKey">Your EasyPost API key.</param>
        public static void Initialize(string apiKey)
        {
            client = new Client(new ClientConfiguration(apiKey));
        }

        /// <summary>
        /// Validates an address using EasyPost's verification API.
        /// Returns a result object indicating whether the address is valid.
        /// </summary>
        /// <param name="street1">Primary street address.</param>
        /// <param name="street2">Secondary address (optional).</param>
        /// <param name="city">City.</param>
        /// <param name="state">State or province.</param>
        /// <param name="zip">Postal code.</param>
        /// <param name="country">Two-letter country code.</param>
        /// <returns>An AddressValidationResult containing the outcome.</returns>
        public static async Task<AddressValidationResult> ValidateAddressAsync(
            string street1,
            string street2,
            string city,
            string state,
            string zip,
            string country)
        {
            var result = new AddressValidationResult();

            var addressParams = new Dictionary<string, object>
            {
                { "street1", street1 },
                { "street2", street2 },
                { "city", city },
                { "state", state },
                { "zip", zip },
                { "country", country }
            };

            try
            {
                // Attempt to create and verify the address.
                Address verifiedAddress = await client.Address.CreateAndVerify(addressParams);
                result.IsValid = true;
                result.VerifiedAddress = verifiedAddress;
            }
            catch (Exception ex)
            {
                // Verification failed; capture the error message.
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
}