using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Coupons.Queries;

public record GetCouponByCodeQuery(string CouponCode) : IRequest<Result<CouponDto>>;
