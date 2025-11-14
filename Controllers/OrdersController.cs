using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;
using EstoqueBackEnd.DTOs;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    // GET: api/Orders/customer/5
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // GET: api/Orders/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<Order>>> GetPendingOrders()
    {
        return await _context.Orders
            .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .OrderBy(o => o.ExpectedDeliveryDate)
            .ToListAsync();
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Converter status string para enum (aceita snake_case e PascalCase)
            var statusValue = orderDto.Status.Replace("_", "");
            if (!Enum.TryParse<OrderStatus>(statusValue, true, out var orderStatus))
            {
                return BadRequest(new { message = $"Status inválido: {orderDto.Status}" });
            }

            // Converter payment method se fornecido (aceita snake_case e PascalCase)
            PaymentMethod? paymentMethod = null;
            if (!string.IsNullOrEmpty(orderDto.PaymentMethod))
            {
                var paymentValue = orderDto.PaymentMethod.Replace("_", "");
                if (Enum.TryParse<PaymentMethod>(paymentValue, true, out var pm))
                {
                    paymentMethod = pm;
                }
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.Parse(orderDto.CustomerId),
                CustomerName = orderDto.CustomerName,
                Subtotal = orderDto.Subtotal,
                DiscountPercentage = orderDto.DiscountPercentage,
                DiscountAmount = orderDto.DiscountAmount,
                TotalAmount = orderDto.TotalAmount,
                OrderDate = orderDto.OrderDate,
                ExpectedDeliveryDate = orderDto.ExpectedDeliveryDate,
                DeliveredDate = orderDto.DeliveredDate,
                Status = orderStatus,
                PaymentMethodValue = paymentMethod,
                Notes = orderDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            // Processar itens
            foreach (var itemDto in orderDto.Items)
            {
                var item = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = Guid.Parse(itemDto.ProductId),
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                order.Items.Add(item);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao criar encomenda", error = ex.Message });
        }
    }

    // PUT: api/Orders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, OrderDto orderDto)
    {
        var existingOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existingOrder == null)
        {
            return NotFound();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Converter status string para enum (aceita snake_case e PascalCase)
            var statusValue = orderDto.Status.Replace("_", "");
            if (!Enum.TryParse<OrderStatus>(statusValue, true, out var orderStatus))
            {
                return BadRequest(new { message = $"Status inválido: {orderDto.Status}" });
            }

            // Converter payment method se fornecido (aceita snake_case e PascalCase)
            PaymentMethod? paymentMethod = null;
            if (!string.IsNullOrEmpty(orderDto.PaymentMethod))
            {
                var paymentValue = orderDto.PaymentMethod.Replace("_", "");
                if (Enum.TryParse<PaymentMethod>(paymentValue, true, out var pm))
                {
                    paymentMethod = pm;
                }
            }

            // Remover itens antigos do banco
            var existingItems = await _context.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToListAsync();
            if (existingItems.Any())
            {
                _context.OrderItems.RemoveRange(existingItems);
            }

            // Adicionar novos itens
            foreach (var itemDto in orderDto.Items)
            {
                var newItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = id,
                    ProductId = Guid.Parse(itemDto.ProductId),
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = itemDto.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderItems.Add(newItem);
            }

            // Atualizar apenas os campos necessários
            existingOrder.CustomerId = Guid.Parse(orderDto.CustomerId);
            existingOrder.CustomerName = orderDto.CustomerName;
            existingOrder.Subtotal = orderDto.Subtotal;
            existingOrder.DiscountPercentage = orderDto.DiscountPercentage;
            existingOrder.DiscountAmount = orderDto.DiscountAmount;
            existingOrder.TotalAmount = orderDto.TotalAmount;
            existingOrder.OrderDate = orderDto.OrderDate;
            existingOrder.ExpectedDeliveryDate = orderDto.ExpectedDeliveryDate;
            existingOrder.Status = orderStatus;
            existingOrder.Notes = orderDto.Notes;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            // Se está sendo marcado como entregue e não tinha data de entrega
            if (orderStatus == OrderStatus.Delivered && existingOrder.DeliveredDate == null)
            {
                existingOrder.DeliveredDate = orderDto.DeliveredDate ?? DateTime.UtcNow;
            }

            // Atualizar método de pagamento se fornecido
            if (paymentMethod.HasValue)
            {
                existingOrder.PaymentMethodValue = paymentMethod;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Erro ao atualizar encomenda", error = ex.Message });
        }
    }

    // PUT: api/Orders/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
        {
            return NotFound();
        }

        // Converter status string para enum (aceita snake_case e PascalCase)
        var statusValue = request.Status.Replace("_", "");
        if (!Enum.TryParse<OrderStatus>(statusValue, true, out var orderStatus))
        {
            return BadRequest(new { message = "Status inválido" });
        }

        order.Status = orderStatus;
        order.UpdatedAt = DateTime.UtcNow;

        // If delivered, set delivered date and payment method
        if (orderStatus == OrderStatus.Delivered)
        {
            order.DeliveredDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.PaymentMethod))
            {
                var paymentValue = request.PaymentMethod.Replace("_", "");
                if (Enum.TryParse<PaymentMethod>(paymentValue, true, out var paymentMethod))
                {
                    order.PaymentMethodValue = paymentMethod;
                }
            }

            // Create a sale record when order is delivered
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                SaleDate = DateTime.UtcNow,
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                PaymentMethodValue = order.PaymentMethodValue ?? PaymentMethod.Pix,
                Status = SaleStatus.Paid,
                FromOrder = true,
                Notes = $"Venda automática da encomenda #{order.Id}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<SaleItem>()
            };

            // Converter itens da encomenda em itens da venda
            foreach (var orderItem in order.Items)
            {
                var saleItem = new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = sale.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.ProductName,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    TotalPrice = orderItem.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                sale.Items.Add(saleItem);

                // Reduzir estoque
                var product = await _context.Products.FindAsync(orderItem.ProductId);
                if (product != null)
                {
                    product.Quantity -= orderItem.Quantity;
                }
            }

            _context.Sales.Add(sale);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OrderExists(Guid id)
    {
        return _context.Orders.Any(e => e.Id == id);
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}
