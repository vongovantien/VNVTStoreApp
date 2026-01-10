using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Orders.Queries;

public record GetMyOrdersQuery(string UserCode, int PageIndex, int PageSize, string? Status) : IRequest<Result<PagedResult<OrderDto>>>;
public record GetOrderByIdQuery(string OrderCode) : IRequest<Result<OrderDto>>;
public record GetAllOrdersQuery(int PageIndex, int PageSize, string? Status, string? Search, Dictionary<string, string>? Filters = null) : IRequest<Result<PagedResult<OrderDto>>>; // Admin
