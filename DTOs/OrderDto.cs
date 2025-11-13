using System.ComponentModel.DataAnnotations;

namespace EstoqueBackEnd.DTOs;

public class OrderDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [Required]
    public DateTime ExpectedDeliveryDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public string Status { get; set; } = "Pending";

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }
}
