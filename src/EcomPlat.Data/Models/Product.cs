using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    /// <summary>
    /// Represents a product in the catalog.
    /// </summary>
    public class Product : UserStateInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique key for the product.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProductKey { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the product.
        /// </summary>
        [MaxLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the currency used for pricing the product.
        /// </summary>
        public Currency PriceCurrency { get; set; }

        /// <summary>
        /// Gets or sets the standard price of the product.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the sale price of the product, if applicable.
        /// If null, the product is not on sale.
        /// </summary>
        public decimal? SalePrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is available for purchase.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the weight of the product in ounces.
        /// </summary>
        public decimal ProductWeightOunces { get; set; }

        /// <summary>
        /// Gets or sets the shipping weight of the product in ounces.
        /// </summary>
        public decimal ShippingWeightOunces { get; set; }

        /// <summary>
        /// Gets or sets the height of the product in inches.
        /// </summary>
        public decimal HeightInches { get; set; }

        /// <summary>
        /// Gets or sets the width of the product in inches.
        /// </summary>
        public decimal WidthInches { get; set; }

        /// <summary>
        /// Gets or sets the length of the product in inches.
        /// </summary>
        public decimal LengthInches { get; set; }

        /// <summary>
        /// Gets or sets the SKU (Stock Keeping Unit) for the product.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity available.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        /// <summary>
        /// Gets or sets the subcategory identifier.
        /// </summary>
        public int SubCategoryId { get; set; }
        public Subcategory SubCategory { get; set; }

        /// <summary>
        /// Gets or sets the collection of images associated with the product.
        /// </summary>
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
