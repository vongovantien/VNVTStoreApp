using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Carts.Commands;

public record AddToCartCommand(string UserCode, string ProductCode, int Quantity, string? Size, string? Color, string? UnitCode = null) : IRequest<Result<CartDto>>;
public record UpdateCartItemCommand(string UserCode, string CartItemCode, int Quantity) : IRequest<Result<CartDto>>;
public record RemoveFromCartCommand(string UserCode, string CartItemCode) : IRequest<Result<CartDto>>;
public record AddMultipleToCartCommand(string UserCode, List<AddCartItemDto> Items) : IRequest<Result<CartDto>>;

public class AddMultipleCartItemsDto
{
    public List<AddCartItemDto> Items { get; set; } = new();
}

public class AddCartItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? UnitCode { get; set; }
}

public class UpdateCartItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
}
