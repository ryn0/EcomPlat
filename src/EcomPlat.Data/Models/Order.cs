using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    /// <summary>
    /// Represents an order placed by a customer.
    /// </summary>
    public class Order : UserStateInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets a customer-friendly, non-sequential order identifier.
        /// </summary>
        [Required]
        public string CustomerOrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the sale price of the order, which is the billed amount before any shipping or additional fees.
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Gets or sets the currency in which the customer is billed (e.g., USD).
        /// </summary>
        public Currency BillingCurrency { get; set; }

        /// <summary>
        /// Gets or sets the currency in which the payment was made
        /// (for example, the customer might be billed in USD but pay in BTC).
        /// </summary>
        public Currency PaymentCurrency { get; set; }

        /// <summary>
        /// Gets or sets the method used by the customer to make the payment (e.g., CreditCard, Crypto).
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the payment processor that handled the transaction (e.g., Stripe, PayPal).
        /// </summary>
        public PaymentProcessor PaymentProcessor { get; set; }

        // Shipping and order summary details

        /// <summary>
        /// Gets or sets the total shipping weight for the order in ounces.
        /// </summary>
        public decimal ShippingWeightOunces { get; set; }

        /// <summary>
        /// Gets or sets the total amount for the order, including items, shipping, and any additional fees.
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the shipping charge applied to the order.
        /// </summary>
        public decimal ShippingAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unknown;

        public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.Unknown;

        public ShippingCarrier ShippingCarrier { get; set; } = ShippingCarrier.Unknown;

        [MaxLength(100)]
        public string ShipmentTrackingId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping method selected (e.g., Standard, Express, Overnight).
        /// </summary>
        public ShippingMethod ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the collection of items included in the order.
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Gets or sets the collection of addresses associated with the order (such as billing and shipping addresses).
        /// </summary>
        public ICollection<OrderAddress> Addresses { get; set; } = new List<OrderAddress>();
    }
}
