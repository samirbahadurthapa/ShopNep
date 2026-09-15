using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Areas.Admin.Controllers
{
    public class ProductsController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category)
                .OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            await PopulateCategoriesAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(product);
            }

            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.ImageUrl = product.ImageUrl;
            existing.CategoryId = product.CategoryId;
            existing.IsActive = product.IsActive;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft delete: keeps past OrderItems valid instead of breaking referential history.
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Product removed from catalog.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
