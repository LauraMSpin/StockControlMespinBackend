using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

public enum SaleStatus
{
    Pending,
    AwaitingPayment,
    Paid,
    Cancelled
}

public enum PaymentMethod
{
    Cash,
    Pix,
    Debit,
    Credit
}

[Table("sales")]
public class Sale
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("discount_percentage")]
    public decimal DiscountPercentage { get; set; } = 0;

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("sale_date")]
    public DateTime SaleDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("payment_method")]
    public string? PaymentMethodValue { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("from_order")]
    public bool FromOrder { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; } = null!;

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
