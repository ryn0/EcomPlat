using EcomPlat.Data.Models;
using EcomPlat.Shipping.Models;

namespace EcomPlat.Web.Converters
{
    public static class OrderToCartConverter
    {
        public static Shipping.Models.ShoppingCart ConvertToCart(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order cannot be null.");
            }

            return new Shipping.Models.ShoppingCart
            {
                Items = order.OrderItems.Select(item => new Shipping.Models.ShoppingCartItem
                {
                    Product = new Shipping.Models.Product // Use the correct `Product` model
                    {
                        ShippingWeightOunces = item.Product?.ShippingWeightOunces ?? 0,
                        LengthInches = item.Product?.LengthInches ?? 0,
                        WidthInches = item.Product?.WidthInches ?? 0,
                        HeightInches = item.Product?.HeightInches ?? 0
                    },
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }
}
