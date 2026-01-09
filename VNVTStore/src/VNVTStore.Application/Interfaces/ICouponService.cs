using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface ICouponService
{
    Task<Result<CouponDto>> ValidateCouponAsync(string couponCode, decimal orderAmount, CancellationToken cancellationToken = default);
}
