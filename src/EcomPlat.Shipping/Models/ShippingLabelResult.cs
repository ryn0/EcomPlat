namespace EcomPlat.Shipping.Models
{
    public class ShippingLabelResult
    {
        public decimal Cost { get; internal set; }
        public string? Currency { get; internal set; }
        public string? RateId { get; internal set; }
        public string? LabelUrl { get; internal set; }
        public string? TrackingCode { get; internal set; }
    }
}