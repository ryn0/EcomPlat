using System.Collections.Generic;

namespace EcomPlat.Utilities.Helpers
{
    public static class CountryHelper
    {
        // A dictionary mapping ISO 2-letter codes to full country names.
        private static readonly Dictionary<string, string> Countries = new Dictionary<string, string>
        {
            { "US", "United States" },
            { "CA", "Canada" },
            { "GB", "United Kingdom" },
            { "FR", "France" },
            { "DE", "Germany" },
            { "JP", "Japan" },
            { "IT", "Italy" },
            { "UA", "Ukraine" },
            { "PL", "Poland" },
            { "LV", "Latvia" },
            { "GR", "Greece" },
            { "MX", "Mexico" },
            { "LT", "Lithuania" },
            { "AT", "Austria" },
            { "CR", "Costa Rica" }
        };

        /// <summary>
        /// Returns a dictionary of country ISO codes to full country names.
        /// </summary>
        public static IDictionary<string, string> GetCountries()
        {
            return Countries;
        }

        /// <summary>
        /// Returns the full country name for a given two-letter ISO code.
        /// If not found, returns the code.
        /// </summary>
        public static string GetCountryName(string isoCode)
        {
            if (string.IsNullOrWhiteSpace(isoCode))
            {
                return string.Empty;
            }
            isoCode = isoCode.ToUpperInvariant();
            return Countries.TryGetValue(isoCode, out string fullName) ? fullName : isoCode;
        }
    }
}
