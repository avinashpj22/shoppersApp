namespace OrderService.Application.DTOs;

/// <summary>
/// DTO for order statistics and analytics.
/// </summary>
public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CanceledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}
