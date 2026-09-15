using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Step 1: show cart summary + saved addresses + payment method choice.
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var cartItems = await _context.CartItems.Include(c => c.Product)
                .Where(c => c.UserId == userId).ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutViewModel
            {
                Cart = new CartViewModel
                {
                    Items = cartItems.Select(c => new CartItemViewModel
                    {
                        CartItemId = c.Id,
                        ProductId = c.ProductId,
                        ProductName = c.Product!.Name,
                        ImageUrl = c.Product.ImageUrl,
                        UnitPrice = c.Product.Price,
                        Quantity = c.Quantity,
                        AvailableStock = c.Product.StockQuantity
                    }).ToList()
                },
                Addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync()
            };

            vm.SelectedAddressId = vm.Addresses.FirstOrDefault(a => a.IsDefault)?.Id ?? vm.Addresses.FirstOrDefault()?.Id;

            return View(vm);
        }

        // Add a new delivery address from the checkout page.
        [HttpGet]
        public IActionResult AddAddress() => View(new AddressFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AddressFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;

            if (model.IsDefault)
            {
                var existing = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
                foreach (var a in existing) a.IsDefault = false;
            }

            _context.Addresses.Add(new Address
            {
                UserId = userId,
                FullName = model.FullName,
                Phone = model.Phone,
                StreetAddress = model.StreetAddress,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                IsDefault = model.IsDefault
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Step 2: create a Pending order from the cart, then send the browser to the
        // chosen payment gateway (or straight to confirmation for Cash on Delivery).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int addressId, PaymentMethod paymentMethod)
        {
            var userId = _userManager.GetUserId(User)!;

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (address == null)
            {
                TempData["Message"] = "Please choose a valid delivery address.";
                return RedirectToAction(nameof(Index));
            }

            var cartItems = await _context.CartItems.Include(c => c.Product)
                .Where(c => c.UserId == userId).ToListAsync();
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var order = new Order
            {
                UserId = userId,
                AddressId = address.Id,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.PendingPayment,
                TransactionUuid = Guid.NewGuid().ToString("N"),
                TotalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity),
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product!.Name,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Reduce stock immediately to prevent overselling while payment is in progress.
            foreach (var c in cartItems)
            {
                c.Product!.StockQuantity = Math.Max(0, c.Product.StockQuantity - c.Quantity);
            }

            // Cash on Delivery needs no gateway — confirm immediately and empty the cart.
            if (paymentMethod == PaymentMethod.CashOnDelivery)
            {
                order.Status = OrderStatus.Processing;
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            await _context.SaveChangesAsync();

            // For eSewa we keep the order as PendingPayment and hand off to PaymentController,
            // which builds the redirect to the gateway. The cart is cleared only once payment succeeds.
            return RedirectToAction("Pay", "Payment", new { orderId = order.Id });
        }
    }
}
