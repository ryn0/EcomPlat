using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Web.Models
{
    public class ProductReviewEditModel
    {
        public int ReviewId { get; set; }

        [Required]
        public string ReviewerName { get; set; } = string.Empty;

        [Required]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsApproved { get; set; }
    }
}
