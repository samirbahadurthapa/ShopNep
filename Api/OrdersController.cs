using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Api;

[ApiController]
[Authorize]
[Route("api")]
public sealed class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<IReadOnlyList<AddressResponse>>> GetAddresses(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var addresses = await _context.Addresses
            .AsNoTracking()
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.IsDefault)
            .ThenBy(address => address.Id)
            .Select(address => ToAddressResponse(address))
            .ToListAsync(cancellationToken);

        return Ok(addresses);
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressResponse>> AddAddress(AddressRequest request, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        if (request.IsDefault)
            await ClearDefaultAddressAsync(userId, cancellationToken);

        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            StreetAddress = request.StreetAddress,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);
        return Created($"/api/addresses/{address.Id}", ToAddressResponse(address));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetOrders(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.Address)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(ToOrderResponse).ToList());
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _context.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .Include(item => item.Address)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        return order is null ? NotFound() : Ok(ToOrderResponse(order));
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderResponse>> PlaceOrder(PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var address = await _context.Addresses.FirstOrDefaultAsync(
            item => item.Id == request.AddressId && item.UserId == userId,
            cancellationToken);
        if (address is null)
            return BadRequest(new { message = "Please choose a valid delivery address." });

        var cartItems = await _context.CartItems
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .ToListAsync(cancellationToken);
        if (cartItems.Count == 0)
            return BadRequest(new { message = "Your cart is empty." });
        if (cartItems.Any(item => item.Product is null || !item.Product.IsActive || item.Quantity > item.Product.StockQuantity))
            return Conflict(new { message = "One or more cart items are no longer available in the requested quantity." });

        var order = new Order
        {
            UserId = userId,
            AddressId = address.Id,
            PaymentMethod = request.PaymentMethod,
            Status = request.PaymentMethod == PaymentMethod.CashOnDelivery
                ? OrderStatus.Processing
                : OrderStatus.PendingPayment,
            TransactionUuid = Guid.NewGuid().ToString("N"),
            TotalAmount = cartItems.Sum(item => item.Product!.Price * item.Quantity),
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        foreach (var item in cartItems)
            item.Product!.StockQuantity -= item.Quantity;

        if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
            _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(order).Reference(item => item.Address).LoadAsync(cancellationToken);
        return Created($"/api/orders/{order.Id}", ToOrderResponse(order));
    }

    private async Task ClearDefaultAddressAsync(string userId, CancellationToken cancellationToken)
    {
        var addresses = await _context.Addresses
            .Where(address => address.UserId == userId && address.IsDefault)
            .ToListAsync(cancellationToken);
        foreach (var address in addresses)
            address.IsDefault = false;
    }

    private static AddressResponse ToAddressResponse(Address address) => new(
        address.Id,
        address.FullName,
        address.Phone,
        address.StreetAddress,
        address.City,
        address.State,
        address.PostalCode,
        address.Country,
        address.IsDefault);

    private static OrderResponse ToOrderResponse(Order order) => new(
        order.Id,
        order.OrderDate,
        order.TotalAmount,
        order.Status,
        order.PaymentMethod,
        order.TransactionUuid,
        ToAddressResponse(order.Address!),
        order.Items.Select(item => new OrderItemResponse(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity)).ToList());
}
