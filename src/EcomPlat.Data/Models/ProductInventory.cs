using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ProductInventory : UserStateInfo
    {
        public int ProductInventoryId { get; set; }

        // Foreign key linking to the Product table.
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // The cost at which you acquired the product (per unit).
        public decimal PurchaseCost { get; set; }

        public Currency PurchaseCurrency { get; set; }

        // The quantity of items purchased at this cost.
        public int Quantity { get; set; }

        public DateTime PurchaseDate { get; set; }

        // The expiration date after which these products are no longer viable.
        public DateTime ExpirationDateUtc { get; set; }

        // Foreign key linking to the Supplier table.
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
    }
}