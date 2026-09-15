using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    [Authorize] // must be logged in to use the cart — keeps the demo simple (no guest-cart merging logic)
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildCartViewModelAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive) return NotFound();

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity = Math.Min(existing.Quantity + quantity, product.StockQuantity);
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = Math.Min(quantity, Math.Max(product.StockQuantity, 1))
                });
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"{product.Name} added to cart.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _context.CartItems.Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item != null)
            {
                var maxStock = item.Product?.StockQuantity ?? 1;
                item.Quantity = Math.Clamp(quantity, 1, Math.Max(maxStock, 1));
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<CartViewModel> BuildCartViewModelAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.Product != null)
                .ToListAsync();

            return new CartViewModel
            {
                Items = items.Select(c => new CartItemViewModel
                {
                    CartItemId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product!.Name,
                    ImageUrl = c.Product.ImageUrl,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity,
                    AvailableStock = c.Product.StockQuantity
                }).ToList()
            };
        }
    }
}
