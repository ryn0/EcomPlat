namespace EcomPlat.Utilities.Helpers
{
    public static class CurrencyFormatter
    {
        public static string Format(decimal value, decimal conversionRate, string currency)
        {
            switch (currency.ToLowerInvariant())
            {
                case "xmr":
                    return string.Format("{0:F7} XMR", value * conversionRate);
                case "btc":
                    return string.Format("{0:F8} BTC", value * conversionRate);
                case "eth":
                    return string.Format("{0:F7} ETH", value * conversionRate);
                case "kas":
                    return string.Format("{0:F6} KAS", value * conversionRate);
                case "usd":
                    return value.ToString("C");
                default:
                    return value.ToString("C");
            }
        }
    }
}
