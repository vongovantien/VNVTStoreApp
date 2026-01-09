using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Coupons.Queries;

public record GetAllCouponsQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize
) : GetPagedQuery<CouponDto>(PageIndex, PageSize);

public record GetCouponByCodeQuery(string CouponCode) : GetByCodeQuery<CouponDto>(CouponCode);
