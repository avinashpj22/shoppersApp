namespace OrderService.Application.DTOs;

/// <summary>
/// Order Data Transfer Object.
/// Used to transfer order data between services and API endpoints.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<OrderLineItemDto> LineItems { get; set; } = new();
}
