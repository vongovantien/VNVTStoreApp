using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Coupons.Commands;

public record ValidateCouponCommand(string CouponCode, decimal OrderAmount) : IRequest<Result<CouponDto>>;
