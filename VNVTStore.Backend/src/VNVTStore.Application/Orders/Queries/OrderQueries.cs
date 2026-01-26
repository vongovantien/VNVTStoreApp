using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Orders.Queries;

public record GetMyOrdersQuery(string userCode, int pageIndex, int pageSize, OrderStatus? status) : IRequest<Result<PagedResult<OrderDto>>>;
public record GetOrderByIdQuery(string orderCode) : IRequest<Result<OrderDto>>;
public record GetAllOrdersQuery(int pageIndex, int pageSize, OrderStatus? status, string? search, Dictionary<string, string>? filters = null) : IRequest<Result<PagedResult<OrderDto>>>; // Admin
public record GetOrderStatsQuery() : IRequest<Result<OrderStatsDto>>;
