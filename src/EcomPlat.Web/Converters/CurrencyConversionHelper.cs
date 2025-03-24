using EcomPlat.Web.Constants;
using Microsoft.Extensions.Caching.Memory;
using NowPayments.API.Interfaces;

namespace EcomPlat.Web.Converters
{
    public static class CurrencyConversionHelper
    {
        public static async Task<(bool showConverted, decimal conversionRate, string selectedCurrency)> GetConversionContextAsync(
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            INowPaymentsService nowPaymentsService)
        {
            var request = httpContextAccessor.HttpContext?.Request;
            string selectedCurrency = request?.Cookies[StringConstants.CookieNameCurrency] ?? Data.Enums.Currency.USD.ToString();

            bool showConverted = !selectedCurrency.Equals(Data.Enums.Currency.USD.ToString(), StringComparison.OrdinalIgnoreCase);
            decimal conversionRate = 1m;

            if (showConverted)
            {
                string cacheKey = $"{Constants.StringConstants.CacheKeyConversion}_{selectedCurrency}";

                if (!memoryCache.TryGetValue(cacheKey, out conversionRate))
                {
                    try
                    {
                        var estimate = await nowPaymentsService.GetEstimatedConversionAsync(
                            1m,
                            Data.Enums.Currency.USD.ToString(),
                            selectedCurrency);

                        conversionRate = estimate.EstimatedAmount;

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                        memoryCache.Set(cacheKey, conversionRate, cacheEntryOptions);
                    }
                    catch
                    {
                        // If conversion fails (e.g. currency not supported), fallback to USD
                        showConverted = false;
                        conversionRate = 1m;
                        selectedCurrency = Data.Enums.Currency.USD.ToString();
                    }
                }
            }

            return (showConverted, conversionRate, selectedCurrency);
        }
    }
}
