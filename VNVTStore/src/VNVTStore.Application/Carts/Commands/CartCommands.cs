using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Carts.Commands;

public record AddToCartCommand(string UserCode, string ProductCode, int Quantity) : IRequest<Result<CartDto>>;
public record UpdateCartItemCommand(string UserCode, string ProductCode, int Quantity) : IRequest<Result<CartDto>>;
public record RemoveFromCartCommand(string UserCode, string ProductCode) : IRequest<Result<CartDto>>;
public record ClearCartCommand(string UserCode) : IRequest<Result<bool>>;

public class AddToCartDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
}
