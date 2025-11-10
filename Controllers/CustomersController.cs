using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        return await _context.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // GET: api/Customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.Sales)
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return customer;
    }

    // GET: api/Customers/birthday-month
    [HttpGet("birthday-month")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetBirthdayCustomers()
    {
        var currentMonth = DateTime.UtcNow.Month;

        return await _context.Customers
            .Where(c => c.BirthDate != null && c.BirthDate.Value.Month == currentMonth)
            .OrderBy(c => c.BirthDate!.Value.Day)
            .ToListAsync();
    }

    // GET: api/Customers/with-jar-credits
    [HttpGet("with-jar-credits")]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersWithJarCredits()
    {
        return await _context.Customers
            .Where(c => c.JarCredits > 0)
            .OrderByDescending(c => c.JarCredits)
            .ToListAsync();
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        customer.Id = Guid.NewGuid();
        customer.CreatedAt = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    // PUT: api/Customers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest();
        }

        _context.Entry(customer).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(id))
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

    // DELETE: api/Customers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        // Verificar se há vendas associadas
        var hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);
        if (hasSales)
        {
            return BadRequest(new { message = "Não é possível excluir um cliente com vendas associadas." });
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Customers/5/jar-credits
    [HttpPost("{id}/jar-credits")]
    public async Task<IActionResult> UpdateJarCredits(Guid id, [FromBody] int credits)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.JarCredits += credits;
        if (customer.JarCredits < 0) customer.JarCredits = 0;

        await _context.SaveChangesAsync();

        return Ok(new { customer.Id, customer.Name, customer.JarCredits });
    }

    private bool CustomerExists(Guid id)
    {
        return _context.Customers.Any(e => e.Id == id);
    }
}
