namespace VNVTStore.Application.DTOs;

public class CartDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public List<CartItemDto> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class CartItemDto
{
    public string Code { get; set; } = null!;
    public string CartCode { get; set; } = null!;
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public DateTime? AddedAt { get; set; }
}
