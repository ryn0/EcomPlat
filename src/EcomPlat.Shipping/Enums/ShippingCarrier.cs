using System.ComponentModel;

namespace EcomPlat.Shipping.Enums
{
    public enum ShippingCarrier
    {
        Unknown = 0,
        [Description("USPS")]
        USPS = 1
    }
}
