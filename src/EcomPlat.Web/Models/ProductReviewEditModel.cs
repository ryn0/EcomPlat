using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Web.Models
{
    public class ProductReviewEditModel
    {
        public int ReviewId { get; set; }

        [Required]
        public string ReviewerName { get; set; }

        [Required]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsApproved { get; set; }
    }
}
