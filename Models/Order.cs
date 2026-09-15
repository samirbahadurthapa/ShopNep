using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public enum OrderStatus
    {
        PendingPayment = 0,
        Paid = 1,
        Failed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6
    }

    public enum PaymentMethod
    {
        Esewa = 0,
        CashOnDelivery = 1
    }

    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

        public PaymentMethod PaymentMethod { get; set; }

        // A unique reference we generate BEFORE redirecting to the payment gateway.
        // eSewa sends this back so we know which order a callback belongs to.
        public string TransactionUuid { get; set; } = string.Empty;

        // The gateway's own reference id, once payment succeeds (for records / refunds).
        public string? GatewayReferenceId { get; set; }

        public int AddressId { get; set; }
        public Address? Address { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
