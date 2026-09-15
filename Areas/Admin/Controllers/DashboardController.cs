using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.PendingPayment);
            ViewBag.Revenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Processing)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            return View();
        }
    }
}
