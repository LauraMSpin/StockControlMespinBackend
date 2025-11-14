using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

public enum OrderStatus
{
    Pending,
    InProduction,
    ReadyForDelivery,
    Delivered,
    Cancelled
}

[Table("orders")]
public class Order
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
    public decimal DiscountPercentage { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("order_date")]
    public DateTime OrderDate { get; set; }

    [Column("expected_delivery_date")]
    public DateTime ExpectedDeliveryDate { get; set; }

    [Column("delivered_date")]
    public DateTime? DeliveredDate { get; set; }

    [Required]
    [Column("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column("payment_method")]
    public PaymentMethod? PaymentMethodValue { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
