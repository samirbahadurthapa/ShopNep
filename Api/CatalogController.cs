using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Api;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CatalogController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(product => product.Name.Contains(search) ||
                                           (product.Description != null && product.Description.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(product => product.CategoryId == categoryId.Value);

        var products = await query
            .OrderBy(product => product.Name)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.ImageUrl,
                product.CategoryId,
                product.Category != null ? product.Category.Name : null))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(item => item.Category)
            .Where(item => item.Id == id && item.IsActive)
            .Select(item => new ProductResponse(
                item.Id,
                item.Name,
                item.Description,
                item.Price,
                item.StockQuantity,
                item.ImageUrl,
                item.CategoryId,
                item.Category != null ? item.Category.Name : null))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name, category.Description, category.ImageUrl))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }
}
