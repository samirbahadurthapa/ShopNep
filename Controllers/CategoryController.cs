using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Simple redirect so /Category/5 behaves like Home filtered by that category —
        // keeps all the paging/search logic in one place (HomeController.Index).
        public IActionResult Index(int id)
        {
            return RedirectToAction("Index", "Home", new { categoryId = id });
        }

        public async Task<IActionResult> All()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(categories);
        }
    }
}
