using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Promotions.Queries;

public record GetAllPromotionsQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber,
    int PageSize = AppConstants.Paging.DefaultPageSize,
    bool? IsActive = null
) : GetPagedQuery<PromotionDto>(PageIndex, PageSize);

public record GetActivePromotionsQuery() : GetAllQuery<PromotionDto>;

public record GetPromotionByCodeQuery(string PromotionCode) : GetByCodeQuery<PromotionDto>(PromotionCode);
