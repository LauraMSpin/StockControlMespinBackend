namespace EstoqueBackEnd.DTOs;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Category { get; set; }
    public string? Fragrance { get; set; }
    public string? Weight { get; set; }
    public decimal? ProductionCost { get; set; }
    public decimal? ProfitMargin { get; set; }
    public List<ProductionMaterialDto>? ProductionMaterials { get; set; }
    public List<PriceHistoryDto>? PriceHistories { get; set; }
}
