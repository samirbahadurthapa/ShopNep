using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    // We copy the product name/price at the time of purchase, so that later
    // changes to the Product (price update, deletion) never rewrite history.
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
