using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaterialsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Materials
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Material>>> GetMaterials()
    {
        return await _context.Materials
            .Include(m => m.ProductionMaterials)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    // GET: api/Materials/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Material>> GetMaterial(Guid id)
    {
        var material = await _context.Materials
            .Include(m => m.ProductionMaterials)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        return material;
    }

    // GET: api/Materials/low-stock
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Material>>> GetLowStockMaterials()
    {
        return await _context.Materials
            .Where(m => m.CurrentStock <= m.LowStockAlert)
            .OrderBy(m => m.CurrentStock)
            .ToListAsync();
    }

    // GET: api/Materials/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Material>>> GetMaterialsByCategory(string category)
    {
        return await _context.Materials
            .Where(m => m.Category != null && m.Category.ToLower() == category.ToLower())
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    // POST: api/Materials
    [HttpPost]
    public async Task<ActionResult<Material>> CreateMaterial(Material material)
    {
        material.Id = Guid.NewGuid();
        material.CreatedAt = DateTime.UtcNow;
        material.UpdatedAt = DateTime.UtcNow;

        // Calculate cost per unit
        if (material.TotalQuantityPurchased > 0)
        {
            material.CostPerUnit = material.TotalCostPaid / material.TotalQuantityPurchased;
        }

        // Initialize current stock with purchased quantity
        material.CurrentStock = material.TotalQuantityPurchased;

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, material);
    }

    // PUT: api/Materials/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, Material material)
    {
        var existingMaterial = await _context.Materials
            .Include(m => m.ProductionMaterials)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existingMaterial == null)
        {
            return NotFound();
        }

        // Atualizar apenas os campos modificáveis
        existingMaterial.Name = material.Name;
        existingMaterial.Unit = material.Unit;
        existingMaterial.TotalQuantityPurchased = material.TotalQuantityPurchased;
        existingMaterial.CurrentStock = material.CurrentStock;
        existingMaterial.LowStockAlert = material.LowStockAlert;
        existingMaterial.TotalCostPaid = material.TotalCostPaid;
        existingMaterial.Category = material.Category;
        existingMaterial.Supplier = material.Supplier;
        existingMaterial.Notes = material.Notes;

        // Recalculate cost per unit
        if (existingMaterial.TotalQuantityPurchased > 0)
        {
            existingMaterial.CostPerUnit = existingMaterial.TotalCostPaid / existingMaterial.TotalQuantityPurchased;
        }

        existingMaterial.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MaterialExists(id))
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

    // POST: api/Materials/5/update-stock
    [HttpPost("{id}/update-stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        material.CurrentStock += request.Quantity;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { material.Id, material.Name, material.CurrentStock });
    }

    // DELETE: api/Materials/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(Guid id)
    {
        var material = await _context.Materials
            .Include(m => m.ProductionMaterials)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        // Check if material is used in any products
        if (material.ProductionMaterials.Any())
        {
            return Conflict(new { 
                message = "Cannot delete material that is used in products",
                productsCount = material.ProductionMaterials.Count
            });
        }

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MaterialExists(Guid id)
    {
        return _context.Materials.Any(e => e.Id == id);
    }
}

public class UpdateStockRequest
{
    public decimal Quantity { get; set; }
}
