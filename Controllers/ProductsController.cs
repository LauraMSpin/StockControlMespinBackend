using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;
using EstoqueBackEnd.DTOs;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products
            .Include(p => p.ProductionMaterials)
            .Include(p => p.PriceHistories)
            .ToListAsync();
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.ProductionMaterials)
            .Include(p => p.PriceHistories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // GET: api/Products/low-stock
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync();
        var threshold = settings?.LowStockThreshold ?? 10;

        return await _context.Products
            .Where(p => p.Quantity <= threshold)
            .OrderBy(p => p.Quantity)
            .ToListAsync();
    }

    // GET: api/Products/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string category)
    {
        return await _context.Products
            .Where(p => p.Category == category)
            .ToListAsync();
    }

    // POST: api/Products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/Products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto productDto)
    {
        var existingProduct = await _context.Products
            .Include(p => p.ProductionMaterials)
            .Include(p => p.PriceHistories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // Atualizar apenas os campos modificáveis
        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;
        existingProduct.Quantity = productDto.Quantity;
        existingProduct.Category = productDto.Category;
        existingProduct.Fragrance = productDto.Fragrance;
        existingProduct.Weight = productDto.Weight;
        existingProduct.ProductionCost = productDto.ProductionCost;
        existingProduct.ProfitMargin = productDto.ProfitMargin;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        // Atualizar ProductionMaterials se foram enviados
        if (productDto.ProductionMaterials != null)
        {
            // Buscar materiais existentes no banco
            var existingMaterials = await _context.ProductionMaterials
                .Where(pm => pm.ProductId == existingProduct.Id)
                .ToListAsync();

            // Remover materiais existentes apenas se houver algum
            if (existingMaterials.Any())
            {
                _context.ProductionMaterials.RemoveRange(existingMaterials);
            }

            // Adicionar novos materiais
            foreach (var materialDto in productDto.ProductionMaterials)
            {
                var newMaterial = new ProductionMaterial
                {
                    Id = Guid.NewGuid(),
                    ProductId = existingProduct.Id,
                    MaterialId = Guid.Parse(materialDto.MaterialId),
                    MaterialName = materialDto.MaterialName,
                    Quantity = materialDto.Quantity,
                    Unit = materialDto.Unit,
                    CostPerUnit = materialDto.CostPerUnit,
                    TotalCost = materialDto.TotalCost,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ProductionMaterials.Add(newMaterial);
            }
        }

        // Atualizar PriceHistories se foram enviados
        if (productDto.PriceHistories != null)
        {
            // Buscar históricos existentes no banco
            var existingHistories = await _context.PriceHistories
                .Where(ph => ph.ProductId == existingProduct.Id)
                .ToListAsync();

            // Remover históricos existentes apenas se houver algum
            if (existingHistories.Any())
            {
                _context.PriceHistories.RemoveRange(existingHistories);
            }

            // Adicionar novos históricos
            foreach (var historyDto in productDto.PriceHistories)
            {
                var newHistory = new PriceHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = existingProduct.Id,
                    Price = historyDto.Price,
                    Date = historyDto.Date,
                    Reason = historyDto.Reason
                };
                _context.PriceHistories.Add(newHistory);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
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

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Products/5/update-stock
    [HttpPost("{id}/update-stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Define a quantidade total, não adiciona
        product.Quantity = quantity;
        await _context.SaveChangesAsync();

        return Ok(new { product.Id, product.Name, product.Quantity });
    }

    private bool ProductExists(Guid id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
