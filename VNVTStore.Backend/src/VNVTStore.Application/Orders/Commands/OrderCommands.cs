using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Orders.Commands;

public record CreateOrderCommand(string? userCode, CreateOrderDto dto) : IRequest<Result<OrderDto>>;
public record UpdateOrderStatusCommand(string orderCode, OrderStatus status) : IRequest<Result<OrderDto>>;
public record CancelOrderCommand(string userCode, string orderCode, string reason) : IRequest<Result<bool>>;

public class CreateOrderDto
{
    public string? AddressCode { get; set; } // If using saved address
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Note { get; set; }
    public string? CouponCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public List<OrderCreationItemDto>? Items { get; set; } // For Guest Checkout
    public string? CartCode { get; set; } // For Cart Checkout
}

public class OrderCreationItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
}

public class UpdateOrderDto
{
    public string? Status { get; set; }
    public string? Note { get; set; }
    public string? ShippingAddress { get; set; }
}
