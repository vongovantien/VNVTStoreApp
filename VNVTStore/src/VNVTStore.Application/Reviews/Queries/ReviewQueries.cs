using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Reviews.Queries;

public record GetProductReviewsQuery(
    string ProductCode,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<ReviewDto>>>;

public record GetUserReviewsQuery(string UserCode) : IRequest<Result<IEnumerable<ReviewDto>>>;

public record GetReviewByCodeQuery(string ReviewCode) : IRequest<Result<ReviewDto>>;
