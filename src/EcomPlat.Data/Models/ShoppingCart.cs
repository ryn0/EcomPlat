using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ShoppingCart : UserStateInfo
    {
        public int ShoppingCartId { get; set; }

        // Optional: For a registered customer, you might store their user ID.
        public string? CustomerId { get; set; }

        // Alternatively, for anonymous users, you could store a session identifier.
        public string? SessionId { get; set; }

        // Navigation property for the cart items.
        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public decimal ItemsTotal => this.Items.Sum(item => item.Product.Price * item.Quantity);
    }
}