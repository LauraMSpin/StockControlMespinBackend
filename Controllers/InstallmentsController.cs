using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstallmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public InstallmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Installments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstallmentPayment>>> GetInstallments()
    {
        return await _context.InstallmentPayments
            .Include(i => i.PaymentStatus)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();
    }

    // GET: api/Installments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<InstallmentPayment>> GetInstallment(Guid id)
    {
        var installment = await _context.InstallmentPayments
            .Include(i => i.PaymentStatus)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (installment == null)
        {
            return NotFound();
        }

        return installment;
    }

    // GET: api/Installments/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<InstallmentPayment>>> GetInstallmentsByCategory(InstallmentCategory category)
    {
        return await _context.InstallmentPayments
            .Where(i => i.Category == category)
            .Include(i => i.PaymentStatus)
            .OrderByDescending(i => i.StartDate)
            .ToListAsync();
    }

    // GET: api/Installments/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<InstallmentPayment>>> GetPendingInstallments()
    {
        var installments = await _context.InstallmentPayments
            .Include(i => i.PaymentStatus)
            .ToListAsync();

        // Filter installments that have unpaid statuses
        var pending = installments.Where(i => 
            i.PaymentStatus.Any(ps => !ps.IsPaid)
        ).ToList();

        return pending;
    }

    // POST: api/Installments
    [HttpPost]
    public async Task<ActionResult<InstallmentPayment>> CreateInstallment(InstallmentPayment installment)
    {
        installment.Id = Guid.NewGuid();
        installment.CreatedAt = DateTime.UtcNow;
        installment.UpdatedAt = DateTime.UtcNow;

        // Create payment status records for each installment
        installment.PaymentStatus = new List<InstallmentPaymentStatus>();
        for (int i = 0; i < installment.Installments; i++)
        {
            installment.PaymentStatus.Add(new InstallmentPaymentStatus
            {
                Id = Guid.NewGuid(),
                InstallmentPaymentId = installment.Id,
                InstallmentNumber = i + 1,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.InstallmentPayments.Add(installment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInstallment), new { id = installment.Id }, installment);
    }

    // PUT: api/Installments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInstallment(Guid id, InstallmentPayment installment)
    {
        if (id != installment.Id)
        {
            return BadRequest();
        }

        installment.UpdatedAt = DateTime.UtcNow;
        _context.Entry(installment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InstallmentExists(id))
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

    // POST: api/Installments/5/toggle-payment/{installmentNumber}
    [HttpPost("{id}/toggle-payment/{installmentNumber}")]
    public async Task<IActionResult> ToggleInstallmentPayment(Guid id, int installmentNumber)
    {
        var installment = await _context.InstallmentPayments
            .Include(i => i.PaymentStatus)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (installment == null)
        {
            return NotFound();
        }

        var paymentStatus = installment.PaymentStatus
            .FirstOrDefault(ps => ps.InstallmentNumber == installmentNumber);

        if (paymentStatus == null)
        {
            return NotFound($"Installment number {installmentNumber} not found");
        }

        // Toggle the paid status
        paymentStatus.IsPaid = !paymentStatus.IsPaid;
        paymentStatus.PaidDate = paymentStatus.IsPaid ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Installments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInstallment(Guid id)
    {
        var installment = await _context.InstallmentPayments
            .Include(i => i.PaymentStatus)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (installment == null)
        {
            return NotFound();
        }

        _context.InstallmentPayments.Remove(installment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool InstallmentExists(Guid id)
    {
        return _context.InstallmentPayments.Any(e => e.Id == id);
    }
}
