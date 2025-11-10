using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpensesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Expenses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        return await _context.Expenses
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    // GET: api/Expenses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
        {
            return NotFound();
        }

        return expense;
    }

    // GET: api/Expenses/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesByCategory(string category)
    {
        return await _context.Expenses
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    // GET: api/Expenses/date-range?startDate=2024-01-01&endDate=2024-12-31
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _context.Expenses
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    // GET: api/Expenses/recurring
    [HttpGet("recurring")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetRecurringExpenses()
    {
        return await _context.Expenses
            .Where(e => e.IsRecurring)
            .OrderBy(e => e.Description)
            .ToListAsync();
    }

    // POST: api/Expenses
    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        expense.Id = Guid.NewGuid();
        expense.CreatedAt = DateTime.UtcNow;

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
    }

    // PUT: api/Expenses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(Guid id, Expense expense)
    {
        if (id != expense.Id)
        {
            return BadRequest();
        }

        _context.Entry(expense).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ExpenseExists(id))
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

    // DELETE: api/Expenses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ExpenseExists(Guid id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}
