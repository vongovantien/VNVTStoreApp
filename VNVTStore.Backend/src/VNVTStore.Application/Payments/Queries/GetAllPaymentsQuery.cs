using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Payments.Queries;

public record GetAllPaymentsQuery(int PageIndex = 1, int PageSize = 10) : IRequest<Result<PagedResult<PaymentDto>>>;
