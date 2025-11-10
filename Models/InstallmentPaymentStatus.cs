using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("installment_payment_status")]
public class InstallmentPaymentStatus
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("installment_payment_id")]
    public Guid InstallmentPaymentId { get; set; }

    [Column("installment_number")]
    public int InstallmentNumber { get; set; }

    [Column("is_paid")]
    public bool IsPaid { get; set; } = false;

    [Column("paid_date")]
    public DateTime? PaidDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("InstallmentPaymentId")]
    public InstallmentPayment InstallmentPayment { get; set; } = null!;
}
