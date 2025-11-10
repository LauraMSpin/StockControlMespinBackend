using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("materials")]
public class Material
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    [Column("total_quantity_purchased")]
    public decimal TotalQuantityPurchased { get; set; } = 0;

    [Column("current_stock")]
    public decimal CurrentStock { get; set; } = 0;

    [Column("low_stock_alert")]
    public decimal LowStockAlert { get; set; } = 0;

    [Column("total_cost_paid")]
    public decimal TotalCostPaid { get; set; } = 0;

    [Column("cost_per_unit")]
    public decimal CostPerUnit { get; set; } = 0;

    [MaxLength(100)]
    [Column("category")]
    public string? Category { get; set; }

    [MaxLength(255)]
    [Column("supplier")]
    public string? Supplier { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProductionMaterial> ProductionMaterials { get; set; } = new List<ProductionMaterial>();
}
