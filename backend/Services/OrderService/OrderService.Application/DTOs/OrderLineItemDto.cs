namespace OrderService.Application.DTOs;

/// <summary>
/// DTO for an order line item.
/// </summary>
public class OrderLineItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}
