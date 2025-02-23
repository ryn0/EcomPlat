// File: ProductImage.cs
namespace EcomPlat.Data.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        // E.g., "Thumbnail", "Medium", "Large"
        public string Size { get; set; }

        // Flag to indicate if this is the main image for the product
        public bool IsMain { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
