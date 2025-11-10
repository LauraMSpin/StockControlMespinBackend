using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Data;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Settings
    [HttpGet]
    public async Task<ActionResult<Setting>> GetSettings()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            return NotFound();
        }

        return settings;
    }

    // PUT: api/Settings
    [HttpPut]
    public async Task<IActionResult> UpdateSettings(Setting setting)
    {
        var existingSettings = await _context.Settings.FirstOrDefaultAsync();
        
        if (existingSettings == null)
        {
            return NotFound();
        }

        existingSettings.LowStockThreshold = setting.LowStockThreshold;
        existingSettings.CompanyName = setting.CompanyName;
        existingSettings.CompanyPhone = setting.CompanyPhone;
        existingSettings.CompanyEmail = setting.CompanyEmail;
        existingSettings.CompanyAddress = setting.CompanyAddress;
        existingSettings.BirthdayDiscount = setting.BirthdayDiscount;
        existingSettings.JarDiscount = setting.JarDiscount;
        existingSettings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
