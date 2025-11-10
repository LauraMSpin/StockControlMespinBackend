using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryPricesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryPricesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/CategoryPrices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryPrice>>> GetCategoryPrices()
    {
        return await _context.CategoryPrices
            .OrderBy(cp => cp.CategoryName)
            .ToListAsync();
    }

    // GET: api/CategoryPrices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryPrice>> GetCategoryPrice(Guid id)
    {
        var categoryPrice = await _context.CategoryPrices.FindAsync(id);

        if (categoryPrice == null)
        {
            return NotFound();
        }

        return categoryPrice;
    }

    // GET: api/CategoryPrices/by-name/{name}
    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<CategoryPrice>> GetCategoryPriceByName(string name)
    {
        var categoryPrice = await _context.CategoryPrices
            .FirstOrDefaultAsync(cp => cp.CategoryName.ToLower() == name.ToLower());

        if (categoryPrice == null)
        {
            return NotFound();
        }

        return categoryPrice;
    }

    // POST: api/CategoryPrices
    [HttpPost]
    public async Task<ActionResult<CategoryPrice>> CreateCategoryPrice(CategoryPrice categoryPrice)
    {
        // Check if category name already exists
        var exists = await _context.CategoryPrices
            .AnyAsync(cp => cp.CategoryName.ToLower() == categoryPrice.CategoryName.ToLower());

        if (exists)
        {
            return Conflict(new { message = "A category with this name already exists" });
        }

        categoryPrice.Id = Guid.NewGuid();
        categoryPrice.CreatedAt = DateTime.UtcNow;
        categoryPrice.UpdatedAt = DateTime.UtcNow;

        _context.CategoryPrices.Add(categoryPrice);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryPrice), new { id = categoryPrice.Id }, categoryPrice);
    }

    // PUT: api/CategoryPrices/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoryPrice(Guid id, CategoryPrice categoryPrice)
    {
        if (id != categoryPrice.Id)
        {
            return BadRequest();
        }

        // Check if another category with the same name exists
        var exists = await _context.CategoryPrices
            .AnyAsync(cp => cp.CategoryName.ToLower() == categoryPrice.CategoryName.ToLower() && cp.Id != id);

        if (exists)
        {
            return Conflict(new { message = "A category with this name already exists" });
        }

        categoryPrice.UpdatedAt = DateTime.UtcNow;
        _context.Entry(categoryPrice).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryPriceExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/CategoryPrices/5/apply-to-products
    [HttpPost("{id}/apply-to-products")]
    public async Task<IActionResult> ApplyToProducts(Guid id)
    {
        var categoryPrice = await _context.CategoryPrices.FindAsync(id);
        if (categoryPrice == null)
        {
            return NotFound();
        }

        // Find all products with this category
        var products = await _context.Products
            .Where(p => p.Category != null && p.Category.ToLower() == categoryPrice.CategoryName.ToLower())
            .ToListAsync();

        // Update all products with the new price
        foreach (var product in products)
        {
            product.Price = categoryPrice.Price;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = $"Price updated for {products.Count} products",
            updatedCount = products.Count
        });
    }

    // DELETE: api/CategoryPrices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryPrice(Guid id)
    {
        var categoryPrice = await _context.CategoryPrices.FindAsync(id);
        if (categoryPrice == null)
        {
            return NotFound();
        }

        _context.CategoryPrices.Remove(categoryPrice);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryPriceExists(Guid id)
    {
        return _context.CategoryPrices.Any(e => e.Id == id);
    }
}
