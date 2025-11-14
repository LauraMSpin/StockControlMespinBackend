using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("price")]
    public decimal Price { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; } = 0;

    [MaxLength(100)]
    [Column("category")]
    public string? Category { get; set; }

    [MaxLength(100)]
    [Column("fragrance")]
    public string? Fragrance { get; set; }

    [MaxLength(50)]
    [Column("weight")]
    public string? Weight { get; set; }

    [Column("production_cost")]
    public decimal? ProductionCost { get; set; }

    [Column("profit_margin")]
    public decimal? ProfitMargin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProductionMaterial> ProductionMaterials { get; set; } = new List<ProductionMaterial>();
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
