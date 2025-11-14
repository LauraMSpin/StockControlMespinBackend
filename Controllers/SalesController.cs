using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;
using EstoqueBackEnd.DTOs;

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
            .Where(s => s.Status == SaleStatus.Pending || s.Status == SaleStatus.AwaitingPayment)
            .OrderBy(s => s.SaleDate)
            .ToListAsync();
    }

    // POST: api/Sales
    [HttpPost]
    public async Task<ActionResult<Sale>> CreateSale([FromBody] CreateOrderDto saleDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Converter status string para enum (aceita snake_case e PascalCase)
            var statusValue = saleDto.Status.Replace("_", "");
            if (!Enum.TryParse<SaleStatus>(statusValue, true, out var saleStatus))
            {
                return BadRequest(new { message = $"Status inválido: {saleDto.Status}" });
            }

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.Parse(saleDto.CustomerId),
                CustomerName = saleDto.CustomerName,
                SaleDate = saleDto.SaleDate,
                Subtotal = saleDto.Subtotal,
                DiscountPercentage = saleDto.DiscountPercentage,
                DiscountAmount = saleDto.DiscountAmount,
                TotalAmount = saleDto.TotalAmount,
                Status = saleStatus,
                Notes = saleDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<SaleItem>()
            };

            // Processar itens
            foreach (var itemDto in saleDto.Items)
            {
                var item = new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = sale.Id,
                    ProductId = Guid.Parse(itemDto.ProductId),
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                sale.Items.Add(item);

                // Atualizar estoque do produto
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Produto {itemDto.ProductName} não encontrado." });
                }

                if (product.Quantity < item.Quantity)
                {
                    return BadRequest(new { message = $"Estoque insuficiente para {itemDto.ProductName}. Disponível: {product.Quantity}" });
                }

                product.Quantity -= item.Quantity;
            }

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
    public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateSaleDto saleDto)
    {
        var existingSale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSale == null)
        {
            return NotFound();
        }

        // Não permitir alterar o status de vendas que já estão pagas
        var statusValue = saleDto.Status.Replace("_", "");
        if (!Enum.TryParse<SaleStatus>(statusValue, true, out var newSaleStatus))
        {
            return BadRequest(new { message = $"Status inválido: {saleDto.Status}" });
        }

        if (existingSale.Status == SaleStatus.Paid && newSaleStatus != SaleStatus.Paid)
        {
            return BadRequest(new { message = "Não é possível alterar o status de vendas pagas." });
        }

        var saleStatus = newSaleStatus;

        // Converter payment method se fornecido (aceita snake_case e PascalCase)
        PaymentMethod? paymentMethod = null;
        if (!string.IsNullOrEmpty(saleDto.PaymentMethod))
        {
            var paymentValue = saleDto.PaymentMethod.Replace("_", "");
            if (Enum.TryParse<PaymentMethod>(paymentValue, true, out var pm))
            {
                paymentMethod = pm;
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Criar dicionário dos itens antigos para comparação
            var oldItemsDict = existingSale.Items.ToDictionary(i => i.ProductId, i => i.Quantity);
            var newItemsDict = saleDto.Items.ToDictionary(i => Guid.Parse(i.ProductId), i => i.Quantity);

            // Restaurar estoque dos itens removidos
            foreach (var oldItem in existingSale.Items)
            {
                if (!newItemsDict.ContainsKey(oldItem.ProductId))
                {
                    // Item foi removido, devolver ao estoque
                    var product = await _context.Products.FindAsync(oldItem.ProductId);
                    if (product != null)
                    {
                        product.Quantity += oldItem.Quantity;
                    }
                }
            }

            // Remover itens antigos do banco
            var existingItems = await _context.SaleItems
                .Where(si => si.SaleId == id)
                .ToListAsync();
            if (existingItems.Any())
            {
                _context.SaleItems.RemoveRange(existingItems);
            }

            // Adicionar novos itens e atualizar estoque
            foreach (var itemDto in saleDto.Items)
            {
                var newItem = new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = id,
                    ProductId = Guid.Parse(itemDto.ProductId),
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                var product = await _context.Products.FindAsync(newItem.ProductId);
                if (product == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"Produto {itemDto.ProductName} não encontrado." });
                }

                // Calcular diferença de quantidade
                int oldQuantity = oldItemsDict.ContainsKey(newItem.ProductId) ? oldItemsDict[newItem.ProductId] : 0;
                int quantityDifference = newItem.Quantity - oldQuantity;

                if (quantityDifference > 0)
                {
                    // Aumentou a quantidade, verificar estoque
                    if (product.Quantity < quantityDifference)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Estoque insuficiente para {itemDto.ProductName}. Disponível: {product.Quantity}" });
                    }
                    product.Quantity -= quantityDifference;
                }
                else if (quantityDifference < 0)
                {
                    // Diminuiu a quantidade, devolver ao estoque
                    product.Quantity += Math.Abs(quantityDifference);
                }
                // Se quantityDifference == 0, não faz nada

                _context.SaleItems.Add(newItem);
            }

            // Atualizar venda
            existingSale.CustomerId = Guid.Parse(saleDto.CustomerId);
            existingSale.CustomerName = saleDto.CustomerName;
            existingSale.Subtotal = saleDto.Subtotal;
            existingSale.DiscountPercentage = saleDto.DiscountPercentage;
            existingSale.DiscountAmount = saleDto.DiscountAmount;
            existingSale.TotalAmount = saleDto.TotalAmount;
            existingSale.SaleDate = saleDto.SaleDate;
            existingSale.Status = saleStatus;
            existingSale.PaymentMethodValue = paymentMethod;
            existingSale.Notes = saleDto.Notes;
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
    public async Task<IActionResult> UpdateSaleStatus(Guid id, [FromBody] UpdateSaleStatusRequest request)
    {
        var sale = await _context.Sales.FindAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        // Converter status string para enum (aceita snake_case e PascalCase)
        var statusValue = request.Status.Replace("_", "");
        if (!Enum.TryParse<SaleStatus>(statusValue, true, out var saleStatus))
        {
            return BadRequest(new { message = "Status inválido" });
        }

        sale.Status = saleStatus;
        
        // Atualizar paymentMethod se fornecido
        if (!string.IsNullOrEmpty(request.PaymentMethod))
        {
            var paymentValue = request.PaymentMethod.Replace("_", "");
            if (Enum.TryParse<PaymentMethod>(paymentValue, true, out var paymentMethod))
            {
                sale.PaymentMethodValue = paymentMethod;
            }
        }
        
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

public class UpdateSaleStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}
