using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Simple image URL (or /images/xyz.jpg under wwwroot) — keeps upload logic out of scope for a beginner project.
        public string? ImageUrl { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
