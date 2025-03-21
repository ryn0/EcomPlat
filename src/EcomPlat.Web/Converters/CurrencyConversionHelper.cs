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
            string selectedCurrency = request?.Cookies["currency"] ?? Data.Enums.Currency.USD.ToString();

            bool showConverted = selectedCurrency.Equals(Data.Enums.Currency.XMR.ToString(), StringComparison.OrdinalIgnoreCase);
            decimal conversionRate = 0m;

            if (showConverted)
            {
                string cacheKey = Constants.StringConstants.CacheKeyConversion;

                if (!memoryCache.TryGetValue(cacheKey, out conversionRate))
                {
                    var estimate = await nowPaymentsService.GetEstimatedConversionAsync(1m, Data.Enums.Currency.USD.ToString(), Data.Enums.Currency.XMR.ToString());
                    conversionRate = estimate.EstimatedAmount;

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    memoryCache.Set(cacheKey, conversionRate, cacheEntryOptions);
                }
            }

            return (showConverted, conversionRate, selectedCurrency);
        }
    }
}
