using System.ComponentModel.DataAnnotations;
using ECommerceApp.Models;

namespace ECommerceApp.Api;

public sealed record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    int CategoryId,
    string? CategoryName);

public sealed record CategoryResponse(int Id, string Name, string? Description, string? ImageUrl);

public sealed record AuthResponse(string Id, string Email, string FullName, IList<string> Roles);

public sealed class RegisterRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed record CartItemResponse(
    int CartItemId,
    int ProductId,
    string ProductName,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock);

public sealed record CartResponse(IReadOnlyList<CartItemResponse> Items, decimal Total);

public sealed class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}

public sealed class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public sealed class AddressRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string Country { get; set; } = "Nepal";

    public bool IsDefault { get; set; }
}

public sealed record AddressResponse(
    int Id,
    string FullName,
    string Phone,
    string StreetAddress,
    string City,
    string? State,
    string? PostalCode,
    string Country,
    bool IsDefault);

public sealed class PlaceOrderRequest
{
    [Range(1, int.MaxValue)]
    public int AddressId { get; set; }

    [EnumDataType(typeof(PaymentMethod))]
    public PaymentMethod PaymentMethod { get; set; }
}

public sealed record OrderItemResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record OrderResponse(
    int Id,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    string TransactionUuid,
    AddressResponse Address,
    IReadOnlyList<OrderItemResponse> Items);
