using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    public class InternalUsage : UserStateInfo
    {
        public int InternalUsageId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int QuantityUsed { get; set; } // How much was consumed
        public DateTime UsageDate { get; set; } = DateTime.UtcNow;

        public string UsedBy { get; set; } // Who used it (e.g., employee or department)

        public string Notes { get; set; } // Optional: Reason for usage
    }

}
