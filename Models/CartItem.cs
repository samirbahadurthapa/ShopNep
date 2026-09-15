namespace ECommerceApp.Models
{
    // Cart is persisted in the database, one row per (user, product).
    // Simple & reliable for a beginner project — survives logout/login, no session juggling.
    public class CartItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
