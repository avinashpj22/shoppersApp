namespace ProductService.Application.DTOs;

/// <summary>
/// DTO for creating a new product.
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
