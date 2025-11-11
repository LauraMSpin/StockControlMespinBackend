namespace EstoqueBackEnd.DTOs;

public class ProductionMaterialDto
{
    public string MaterialId { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
    public decimal TotalCost { get; set; }
}
