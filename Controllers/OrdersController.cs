using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

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
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Product)
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
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // GET: api/Orders/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<Order>>> GetPendingOrders()
    {
        return await _context.Orders
            .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .OrderBy(o => o.ExpectedDeliveryDate)
            .ToListAsync();
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        order.Id = Guid.NewGuid();
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    // PUT: api/Orders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, Order order)
    {
        var existingOrder = await _context.Orders.FindAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        // Atualizar apenas os campos necessários
        existingOrder.CustomerId = order.CustomerId;
        existingOrder.CustomerName = order.CustomerName;
        existingOrder.ProductId = order.ProductId;
        existingOrder.ProductName = order.ProductName;
        existingOrder.Quantity = order.Quantity;
        existingOrder.UnitPrice = order.UnitPrice;
        existingOrder.TotalAmount = order.TotalAmount;
        existingOrder.OrderDate = order.OrderDate;
        existingOrder.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
        existingOrder.Status = order.Status;
        existingOrder.Notes = order.Notes;
        existingOrder.UpdatedAt = DateTime.UtcNow;

        // Se está sendo marcado como entregue e não tinha data de entrega
        if (order.Status == OrderStatus.Delivered && existingOrder.DeliveredDate == null && order.DeliveredDate != null)
        {
            existingOrder.DeliveredDate = DateTime.UtcNow;
        }

        // Atualizar método de pagamento se fornecido
        if (order.PaymentMethodValue.HasValue)
        {
            existingOrder.PaymentMethodValue = order.PaymentMethodValue;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OrderExists(id))
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

    // PUT: api/Orders/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
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
                Subtotal = order.TotalAmount,
                TotalAmount = order.TotalAmount,
                PaymentMethodValue = order.PaymentMethodValue ?? PaymentMethod.Pix,
                Status = SaleStatus.Paid,
                FromOrder = true,
                Notes = $"Venda automática da encomenda #{order.Id}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<SaleItem>
                {
                    new SaleItem
                    {
                        Id = Guid.NewGuid(),
                        SaleId = Guid.NewGuid(),
                        ProductId = order.ProductId,
                        ProductName = order.ProductName,
                        Quantity = order.Quantity,
                        UnitPrice = order.UnitPrice,
                        TotalPrice = order.TotalAmount,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            _context.Sales.Add(sale);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
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
