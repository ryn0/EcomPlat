namespace EcomPlat.Utilities.Helpers
{
    public static class CurrencyFormatter
    {
        public static string Format(decimal value, decimal conversionRate, string currency)
        {
            switch (currency.ToLowerInvariant())
            {
                case "xmr":
                    return string.Format("{0:F6} XMR", value * conversionRate);
                case "usd":
                    return value.ToString("C");
                default:
                    return value.ToString("C");
            }
        }
    }
}
