using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("production_materials")]
public class ProductionMaterial
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("material_id")]
    public Guid MaterialId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("material_name")]
    public string MaterialName { get; set; } = string.Empty;

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    [Column("cost_per_unit")]
    public decimal CostPerUnit { get; set; }

    [Column("total_cost")]
    public decimal TotalCost { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [ForeignKey("MaterialId")]
    public Material Material { get; set; } = null!;
}
