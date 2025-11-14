using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

public enum ExpenseCategory
{
    Production,
    Investment,
    FixedCost,
    VariableCost,
    Other
}

[Table("expenses")]
public class Expense
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("category")]
    public ExpenseCategory Category { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("is_recurring")]
    public bool IsRecurring { get; set; } = false;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
