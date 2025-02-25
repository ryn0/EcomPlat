using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ProductImage : UserStateInfo
    {
        public int ProductImageId { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public ImageSize Size { get; set; }

        // Flag to indicate if this is the main image for the product.
        public bool IsMain { get; set; }

        // Foreign key to Product.
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Sequential order for displaying images; must be unique for each product.
        public int DisplayOrder { get; set; }

        public Guid ImageGroupGuid { get; set; }
    }
}