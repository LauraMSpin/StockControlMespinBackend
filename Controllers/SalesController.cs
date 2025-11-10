using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Sales
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    // GET: api/Sales/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Sale>> GetSale(Guid id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            return NotFound();
        }

        return sale;
    }

    // GET: api/Sales/customer/5
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Sale>>> GetSalesByCustomer(Guid customerId)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    // GET: api/Sales/today
    [HttpGet("today")]
    public async Task<ActionResult<IEnumerable<Sale>>> GetTodaySales()
    {
        var today = DateTime.UtcNow.Date;
        
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.SaleDate.Date == today)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    // GET: api/Sales/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<Sale>>> GetPendingSales()
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.Status == "pending" || s.Status == "awaiting_payment")
            .OrderBy(s => s.SaleDate)
            .ToListAsync();
    }

    // POST: api/Sales
    [HttpPost]
    public async Task<ActionResult<Sale>> CreateSale(Sale sale)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            sale.Id = Guid.NewGuid();
            sale.CreatedAt = DateTime.UtcNow;
            sale.UpdatedAt = DateTime.UtcNow;

            // Processar itens
            foreach (var item in sale.Items)
            {
                item.Id = Guid.NewGuid();
                item.SaleId = sale.Id;
                item.CreatedAt = DateTime.UtcNow;

                // Atualizar estoque do produto
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Produto {item.ProductName} não encontrado." });
                }

                if (product.Quantity < item.Quantity)
                {
                    return BadRequest(new { message = $"Estoque insuficiente para {item.ProductName}. Disponível: {product.Quantity}" });
                }

                product.Quantity -= item.Quantity;
            }

            // Atualizar créditos de potes se aplicável (se houver lógica específica)
            // Isso seria feito no frontend, mas pode ser validado aqui

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao criar venda", error = ex.Message });
        }
    }

    // PUT: api/Sales/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSale(Guid id, Sale sale)
    {
        if (id != sale.Id)
        {
            return BadRequest();
        }

        var existingSale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSale == null)
        {
            return NotFound();
        }

        // Não permitir editar vendas pagas
        if (existingSale.Status == "paid")
        {
            return BadRequest(new { message = "Vendas pagas não podem ser editadas." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Restaurar estoque dos itens antigos
            foreach (var oldItem in existingSale.Items)
            {
                var product = await _context.Products.FindAsync(oldItem.ProductId);
                if (product != null)
                {
                    product.Quantity += oldItem.Quantity;
                }
            }

            // Remover itens antigos
            _context.SaleItems.RemoveRange(existingSale.Items);

            // Adicionar novos itens e atualizar estoque
            foreach (var newItem in sale.Items)
            {
                newItem.Id = Guid.NewGuid();
                newItem.SaleId = sale.Id;
                newItem.CreatedAt = DateTime.UtcNow;

                var product = await _context.Products.FindAsync(newItem.ProductId);
                if (product == null || product.Quantity < newItem.Quantity)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"Estoque insuficiente para {newItem.ProductName}" });
                }

                product.Quantity -= newItem.Quantity;
            }

            // Atualizar venda
            existingSale.CustomerId = sale.CustomerId;
            existingSale.CustomerName = sale.CustomerName;
            existingSale.Subtotal = sale.Subtotal;
            existingSale.DiscountPercentage = sale.DiscountPercentage;
            existingSale.DiscountAmount = sale.DiscountAmount;
            existingSale.TotalAmount = sale.TotalAmount;
            existingSale.SaleDate = sale.SaleDate;
            existingSale.Status = sale.Status;
            existingSale.PaymentMethodValue = sale.PaymentMethodValue;
            existingSale.Notes = sale.Notes;
            existingSale.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao atualizar venda", error = ex.Message });
        }
    }

    // PATCH: api/Sales/5/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateSaleStatus(Guid id, [FromBody] string status)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        var validStatuses = new[] { "pending", "awaiting_payment", "paid", "cancelled" };
        if (!validStatuses.Contains(status))
        {
            return BadRequest(new { message = "Status inválido" });
        }

        sale.Status = status;
        sale.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(sale);
    }

    // DELETE: api/Sales/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSale(Guid id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            return NotFound();
        }

        // Não permitir excluir vendas pagas
        if (sale.Status == "paid")
        {
            return BadRequest(new { message = "Vendas pagas não podem ser excluídas." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Restaurar estoque
            foreach (var item in sale.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                }
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao excluir venda", error = ex.Message });
        }
    }
}
