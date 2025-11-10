using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("price_history")]
public class PriceHistory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column("reason")]
    public string? Reason { get; set; }

    // Navigation properties
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}
