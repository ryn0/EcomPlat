using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Web.Models
{
    public class CheckoutModel
    {
        [ValidateNever]
        public string Email { get; set; } = string.Empty;
    }
}
