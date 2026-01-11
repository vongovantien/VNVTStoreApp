using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Orders.Commands;

public record CreateOrderCommand(string UserCode, CreateOrderDto Dto) : IRequest<Result<OrderDto>>;
public record UpdateOrderStatusCommand(string OrderCode, string Status) : IRequest<Result<OrderDto>>;
public record CancelOrderCommand(string UserCode, string OrderCode, string Reason) : IRequest<Result<bool>>;

public class CreateOrderDto
{
    public string? AddressCode { get; set; } // If using saved address
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Note { get; set; }
    public string? CouponCode { get; set; }
    public string PaymentMethod { get; set; } = "COD"; // COD, VNPay, etc.
}
