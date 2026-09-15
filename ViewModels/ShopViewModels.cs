using System.ComponentModel.DataAnnotations;
using ECommerceApp.Models;

namespace ECommerceApp.ViewModels
{
    // Powers the Home/Category listing pages: products + the data needed for filters & paging.
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 8;
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
    }

    public class AddressFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(200)]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; } = "Nepal";

        public bool IsDefault { get; set; }
    }

    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();
        public int? SelectedAddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = Models.PaymentMethod.Esewa;
    }
}
