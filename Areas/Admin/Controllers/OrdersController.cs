using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Areas.Admin.Controllers
{
    public class OrdersController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var query = _context.Orders.Include(o => o.User).Include(o => o.Items).AsQueryable();
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            ViewBag.SelectedStatus = status;
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Order status updated.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
