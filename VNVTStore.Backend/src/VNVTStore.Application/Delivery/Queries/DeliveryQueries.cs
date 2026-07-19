using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Delivery.Queries;

public record GetDeliveryByOrderQuery(string OrderCode) : IRequest<Result<DeliveryDto>>;

public record GetDeliveryHistoryQuery(string DeliveryCode) : IRequest<Result<List<DeliveryHistoryDto>>>;
