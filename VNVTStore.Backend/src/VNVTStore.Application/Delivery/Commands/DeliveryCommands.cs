using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Delivery.Commands;

public record AssignDeliveryCommand(string UserCode, string OrderCode, AssignDeliveryDto Dto) : IRequest<Result<DeliveryDto>>;

public record UpdateDeliveryStatusCommand(string UserCode, string DeliveryCode, string Status, string? Note, string? Location) : IRequest<Result<DeliveryDto>>;
