using System.ComponentModel;

namespace EcomPlat.Shipping.Enums
{
    public enum UspsServices
    {
        Unknown = 0,
        [Description("First")]
        First = 1,
        [Description("Priority")]
        Priority = 2,
        [Description("Express")]
        Express = 3,
        [Description("GroundAdvantage")]
        GroundAdvantage = 4,
        [Description("LibraryMail")]
        LibraryMail = 5,
        [Description("MediaMail")]
        MediaMail = 6,
        [Description("FirstClassMailInternational")]
        FirstClassMailInternational = 6,
        [Description("FirstClassPackageInternationalService")]
        FirstClassPackageInternationalService = 7,
        [Description("PriorityMailInternational")]
        PriorityMailInternational = 8,
        [Description("ExpressMailInternational")]
        ExpressMailInternational = 9
    }
}
