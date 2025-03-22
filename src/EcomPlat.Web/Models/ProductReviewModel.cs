using System.ComponentModel.DataAnnotations;

namespace EcomPlat.Web.Models
{
    public class ProductReviewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        public string Captcha { get; set; }
    }
}
