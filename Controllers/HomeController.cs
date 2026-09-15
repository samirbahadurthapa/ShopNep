using ECommerceApp.Data;
using ECommerceApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Home page: product grid with optional category filter + search box + paging.
        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
        {
            const int pageSize = 8;

            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                SelectedCategoryId = categoryId,
                SearchTerm = search,
                CurrentPage = page,
                TotalPages = Math.Max(totalPages, 1),
                PageSize = pageSize
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
