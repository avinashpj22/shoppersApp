namespace OrderService.Application.DTOs;

/// <summary>
/// DTO for creating a new order.
/// </summary>
public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderLineItemDto> LineItems { get; set; } = new();
}
