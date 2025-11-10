using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueBackEnd.Models;

[Table("settings")]
public class Setting
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("low_stock_threshold")]
    public int LowStockThreshold { get; set; } = 10;

    [Required]
    [MaxLength(255)]
    [Column("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("company_phone")]
    public string? CompanyPhone { get; set; }

    [MaxLength(255)]
    [Column("company_email")]
    public string? CompanyEmail { get; set; }

    [Column("company_address")]
    public string? CompanyAddress { get; set; }

    [Column("birthday_discount")]
    public decimal BirthdayDiscount { get; set; } = 0;

    [Column("jar_discount")]
    public decimal JarDiscount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
