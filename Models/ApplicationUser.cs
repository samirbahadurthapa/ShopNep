using Microsoft.AspNetCore.Identity;

namespace ECommerceApp.Models
{
    // Extends the built-in Identity user with a couple of extra fields we need.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
