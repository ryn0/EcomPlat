namespace EcomPlat.Shipping.Models
{
    public class ShoppingCart
    {

        // Navigation property for the cart items.
        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
    }
}