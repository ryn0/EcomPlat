using EasyPost.Models.API;

namespace EcomPlat.Shipping.Models
{
    public class AddressValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public Address VerifiedAddress { get; set; }
    }
}
