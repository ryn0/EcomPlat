using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ShoppingCartItem : UserStateInfo
    {
        public int ShoppingCartItemId { get; set; }

        // Foreign key linking back to the ShoppingCart table.
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }

        // Foreign key to the Product table.
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Quantity of the product in the cart.
        public int Quantity { get; set; }
    }
}