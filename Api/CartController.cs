using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Api;

[ApiController]
[Authorize]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart(CancellationToken cancellationToken)
    {
        return Ok(await BuildCartAsync(cancellationToken));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var product = await _context.Products.FirstOrDefaultAsync(
            item => item.Id == request.ProductId && item.IsActive,
            cancellationToken);

        if (product is null)
            return NotFound(new { message = "Product not found." });
        if (product.StockQuantity < 1)
            return Conflict(new { message = "Product is out of stock." });

        var item = await _context.CartItems.FirstOrDefaultAsync(
            cartItem => cartItem.UserId == userId && cartItem.ProductId == product.Id,
            cancellationToken);

        if (item is null)
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = product.Id,
                Quantity = Math.Min(request.Quantity, product.StockQuantity)
            });
        }
        else
        {
            item.Quantity = Math.Min(item.Quantity + request.Quantity, product.StockQuantity);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(await BuildCartAsync(cancellationToken));
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(
        int cartItemId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.CartItems
            .Include(cartItem => cartItem.Product)
            .FirstOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId, cancellationToken);

        if (item is null)
            return NotFound();

        item.Quantity = Math.Clamp(request.Quantity, 1, Math.Max(item.Product?.StockQuantity ?? 1, 1));
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(await BuildCartAsync(cancellationToken));
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<ActionResult<CartResponse>> RemoveItem(int cartItemId, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.CartItems.FirstOrDefaultAsync(
            cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId,
            cancellationToken);

        if (item is null)
            return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(await BuildCartAsync(cancellationToken));
    }

    private async Task<CartResponse> BuildCartAsync(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User)!;
        var items = await _context.CartItems
            .AsNoTracking()
            .Include(cartItem => cartItem.Product)
            .Where(cartItem => cartItem.UserId == userId && cartItem.Product != null)
            .Select(cartItem => new CartItemResponse(
                cartItem.Id,
                cartItem.ProductId,
                cartItem.Product!.Name,
                cartItem.Product.ImageUrl,
                cartItem.Product.Price,
                cartItem.Quantity,
                cartItem.Product.StockQuantity))
            .ToListAsync(cancellationToken);

        return new CartResponse(items, items.Sum(item => item.UnitPrice * item.Quantity));
    }
}
