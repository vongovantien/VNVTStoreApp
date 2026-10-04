using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Delivery.Queries;

public record GetDeliveryByOrderQuery(string OrderCode) : IRequest<Result<DeliveryDto>>;

public record GetDeliveryHistoryQuery(string DeliveryCode) : IRequest<Result<List<DeliveryHistoryDto>>>;

public record GetDeliveriesQuery(
    string? Status = null,
    string? Search = null,
    string? ShipperCode = null,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<DeliveryDto>>>;
