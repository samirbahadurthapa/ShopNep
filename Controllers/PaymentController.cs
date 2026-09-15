using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEsewaPaymentService _esewa;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEsewaPaymentService esewa)
        {
            _context = context;
            _userManager = userManager;
            _esewa = esewa;
        }

        // Shows a short "Redirecting to payment gateway..." page that auto-submits the
        // right form/redirect depending on which method the customer picked at checkout.
        public async Task<IActionResult> Pay(int orderId)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return NotFound();

            if (order.PaymentMethod == PaymentMethod.Esewa)
            {
                var formData = _esewa.BuildPaymentForm(order.TotalAmount, order.TransactionUuid);
                return View("EsewaRedirect", formData);
            }

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }

        // ---- eSewa callbacks ----
        // eSewa redirects here with ?data=<base64 JSON> after the customer completes payment.
        public async Task<IActionResult> EsewaSuccess(string data)
        {
            var (isValid, transactionUuid, transactionCode, status) = _esewa.DecodeAndVerify(data);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.TransactionUuid == transactionUuid);
            if (order == null) return NotFound();

            if (isValid)
            {
                order.Status = OrderStatus.Paid;
                order.GatewayReferenceId = transactionCode;
                await ClearCartAsync(order.UserId);
                await _context.SaveChangesAsync();
                return RedirectToAction("Success", new { orderId = order.Id });
            }

            order.Status = OrderStatus.Failed;
            await RestoreStockAsync(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Failure", new { orderId = order.Id, reason = "Signature verification failed." });
        }

        public async Task<IActionResult> EsewaFailure(string? data)
        {
            // eSewa can also redirect here directly if the user cancels/fails on their end.
            Order? order = null;
            if (!string.IsNullOrEmpty(data))
            {
                var (_, transactionUuid, _, _) = _esewa.DecodeAndVerify(data);
                order = await _context.Orders.FirstOrDefaultAsync(o => o.TransactionUuid == transactionUuid);
            }

            if (order != null)
            {
                order.Status = OrderStatus.Failed;
                await RestoreStockAsync(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Failure", new { orderId = order.Id });
            }

            return RedirectToAction("Failure", new { orderId = 0 });
        }

        // ---- Shared result pages ----
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Failure(int orderId, string? reason)
        {
            ViewBag.Reason = reason;
            var order = orderId > 0 ? await _context.Orders.FindAsync(orderId) : null;
            return View(order);
        }

        private async Task ClearCartAsync(string userId)
        {
            var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(items);
        }

        private async Task RestoreStockAsync(Order order)
        {
            var items = await _context.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync();
            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null) product.StockQuantity += item.Quantity;
            }
        }
    }
}
