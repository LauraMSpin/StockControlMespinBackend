using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

public enum InstallmentCategory
{
    Production,
    Investment,
    Equipment,
    Other
}

[Table("installment_payments")]
public class InstallmentPayment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("installments")]
    public int Installments { get; set; }

    [Column("current_installment")]
    public int CurrentInstallment { get; set; } = 1;

    [Column("installment_amount")]
    public decimal InstallmentAmount { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("category")]
    public InstallmentCategory Category { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<InstallmentPaymentStatus> PaymentStatus { get; set; } = new List<InstallmentPaymentStatus>();
}
