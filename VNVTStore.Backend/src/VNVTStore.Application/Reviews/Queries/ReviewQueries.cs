using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Reviews.Queries;

public record GetProductReviewsQuery(
    string ProductCode, 
    int PageIndex = AppConstants.Paging.DefaultPageNumber, 
    int PageSize = AppConstants.Paging.DefaultPageSize
) : GetPagedQuery<ReviewDto>(PageIndex, PageSize);

public record GetUserReviewsQuery(string UserCode) : GetAllQuery<ReviewDto>;

public record GetReviewByCodeQuery(string Code) : GetByCodeQuery<ReviewDto>(Code);

public record GetAllReviewsQuery(
    int PageIndex = AppConstants.Paging.DefaultPageNumber, 
    int PageSize = AppConstants.Paging.DefaultPageSize, 
    bool? IsApproved = null
) : GetPagedQuery<ReviewDto>(PageIndex, PageSize);
