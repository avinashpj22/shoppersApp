namespace OrderService.Application.DTOs;

/// <summary>
/// DTO for creating an order line item.
/// </summary>
public class CreateOrderLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
