namespace EstoqueBackEnd.DTOs;

public class PriceHistoryDto
{
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
}
