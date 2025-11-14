using System.ComponentModel.DataAnnotations;

namespace EstoqueBackEnd.DTOs;

public class OrderItemDto
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }
}

public class OrderDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    [Required]
    public decimal Subtotal { get; set; }

    public decimal DiscountPercentage { get; set; }

    public decimal DiscountAmount { get; set; }

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
