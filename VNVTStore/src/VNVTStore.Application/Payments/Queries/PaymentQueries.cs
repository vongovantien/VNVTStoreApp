using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Payments.Queries;

public record GetPaymentByOrderQuery(string OrderCode) : IRequest<Result<PaymentDto>>;

public record GetMyPaymentsQuery() : IRequest<Result<IEnumerable<PaymentDto>>>;
