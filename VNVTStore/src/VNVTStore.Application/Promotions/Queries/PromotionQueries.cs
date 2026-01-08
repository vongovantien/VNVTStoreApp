using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Promotions.Queries;

public record GetAllPromotionsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    bool? IsActive = null
) : IRequest<Result<PagedResult<PromotionDto>>>;

public record GetActivePromotionsQuery() : IRequest<Result<IEnumerable<PromotionDto>>>;

public record GetPromotionByCodeQuery(string PromotionCode) : IRequest<Result<PromotionDto>>;
