using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Application.Reviews.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Dapper;
using System.Data;

namespace VNVTStore.Application.Reviews.Handlers;

public class ReviewHandlers : BaseHandler<TblReview>,
    IRequestHandler<CreateCommand<CreateReviewDto, ReviewDto>, Result<ReviewDto>>,
    IRequestHandler<UpdateCommand<UpdateReviewDto, ReviewDto>, Result<ReviewDto>>,
    IRequestHandler<DeleteCommand<TblReview>, Result>,
    IRequestHandler<GetProductReviewsQuery, Result<PagedResult<ReviewDto>>>,
    IRequestHandler<GetUserReviewsQuery, Result<IEnumerable<ReviewDto>>>,
    IRequestHandler<GetByCodeQuery<ReviewDto>, Result<ReviewDto>>,
    IRequestHandler<ApproveReviewCommand, Result>,
    IRequestHandler<RejectReviewCommand, Result>,
    IRequestHandler<ReplyReviewCommand, Result>,
    IRequestHandler<GetAllReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IRepository<TblOrderItem> _orderItemRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBaseUrlService _baseUrlService;

    public ReviewHandlers(
        IRepository<TblReview> reviewRepository,
        IRepository<TblOrderItem> orderItemRepository,
        IRepository<TblProduct> productRepository,
        IRepository<TblOrder> orderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        IBaseUrlService baseUrlService) : base(reviewRepository, unitOfWork, mapper, dapperContext)
    {
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _baseUrlService = baseUrlService;
    }

    private async Task UpdateProductRatingAsync(string productCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(productCode)) return;

        var reviews = await _repository.AsQueryable()
            .Where(r => r.ProductCode == productCode && r.IsApproved == true && r.IsActive == true)
            .ToListAsync(cancellationToken);

        var count = reviews.Count;
        var avgRating = count > 0 ? (decimal)reviews.Average(r => r.Rating ?? 0) : 0;

        var product = await _productRepository.GetByCodeAsync(productCode, cancellationToken);
        if (product != null)
        {
            product.UpdateRating(Math.Round(avgRating, 2), count);
            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
    }

    public async Task<Result<ReviewDto>> Handle(CreateCommand<CreateReviewDto, ReviewDto> request, CancellationToken cancellationToken)
    {
        string? productCode = string.IsNullOrEmpty(request.Dto.ProductCode) ? null : request.Dto.ProductCode;
        string? orderItemCode = string.IsNullOrEmpty(request.Dto.OrderItemCode) ? null : request.Dto.OrderItemCode;
        
        // If order item is provided, validate and link
        if (!string.IsNullOrEmpty(orderItemCode))
        {
            var orderItem = await _orderItemRepository.GetByCodeAsync(orderItemCode, cancellationToken);

            if (orderItem == null)
                return Result.Failure<ReviewDto>(Error.NotFound(VNVTStore.Application.Common.MessageConstants.OrderItem, orderItemCode));

            // Check if user owns the order
            var order = await _orderRepository.GetByCodeAsync(orderItem.OrderCode, cancellationToken);

            if (order != null && order.UserCode != request.Dto.UserCode)
            {
                return Result.Failure<ReviewDto>(Error.Forbidden(VNVTStore.Application.Common.MessageConstants.Forbidden));
            }
            
            productCode = orderItem.ProductCode;
        }

        // Check if already reviewed (by Either ProductCode or OrderItemCode)
        var existingReview = await _repository.FindAsync(
            r => (orderItemCode != null && r.OrderItemCode == orderItemCode) || 
                 (productCode != null && r.ProductCode == productCode && r.UserCode == request.Dto.UserCode),
            cancellationToken);

        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Conflict(VNVTStore.Application.Common.MessageConstants.ReviewAlreadyExists));

        var result = await CreateAsync<CreateReviewDto, ReviewDto>(
            request.Dto,
            cancellationToken,
            r => {
                r.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                r.IsApproved = true; // Auto-approve
                r.CreatedAt = DateTime.UtcNow;
                r.ProductCode = productCode;
                r.OrderItemCode = orderItemCode;
            });

        if (result.IsSuccess && !string.IsNullOrEmpty(productCode))
        {
            await UpdateProductRatingAsync(productCode, cancellationToken);
        }

        return result;
    }

    public async Task<Result<ReviewDto>> Handle(UpdateCommand<UpdateReviewDto, ReviewDto> request, CancellationToken cancellationToken)
    {
        var review = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
            return Result.Failure<ReviewDto>(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        var userCode = _currentUser.UserCode;
        if (review.UserCode != userCode)
            return Result.Failure<ReviewDto>(Error.Forbidden("Cannot update another user's review"));

        return await UpdateAsync<UpdateReviewDto, ReviewDto>(
            request.Code,
            request.Dto,
            VNVTStore.Application.Common.MessageConstants.Review,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblReview> request, CancellationToken cancellationToken)
    {
        var review = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
            return Result.Failure(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;

        var productCode = review.ProductCode;
        var result = await DeleteAsync(request.Code, VNVTStore.Application.Common.MessageConstants.Review, cancellationToken, softDelete: false);
        
        if (result.IsSuccess && !string.IsNullOrEmpty(productCode))
        {
            await UpdateProductRatingAsync(productCode, cancellationToken);
        }

        return result;
    }

    private void TransformAvatars(IEnumerable<ReviewDto>? reviews)
    {
        if (reviews == null) return;
        var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
        foreach (var review in reviews)
        {
            if (!string.IsNullOrEmpty(review.UserAvatar))
            {
                review.UserAvatar = review.UserAvatar.StartsWith("http") ? review.UserAvatar : $"{baseUrl}/{review.UserAvatar.TrimStart('/')}";
            }
            if (review.Replies != null && review.Replies.Any())
            {
                TransformAvatars(review.Replies);
            }
        }
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var searchFields = new List<SearchDTO>
        {
            new SearchDTO { SearchField = "ProductCode", SearchValue = request.ProductCode, SearchCondition = SearchCondition.Equal },
            new SearchDTO { SearchField = "ParentCode", SearchValue = null, SearchCondition = SearchCondition.IsNull }
        };

        var result = await GetPagedDapperAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            searchFields,
            null, // Sort
            new List<ReferenceTable> {
                new ReferenceTable { TableName = "TblUser", AliasName = "UserName", ForeignKeyCol = "UserCode", ColumnName = "FullName" },
                new ReferenceTable { TableName = "TblUser", AliasName = "UserAvatar", ForeignKeyCol = "UserCode", ColumnName = "AvatarUrl" },
                new ReferenceTable { TableName = "TblProduct", AliasName = "ProductName", ForeignKeyCol = "ProductCode", ColumnName = "Name", IsJoinThrough = "OrderItemCodeNavigation" }
            },
            request.Fields,
            cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            var parentCodes = result.Value.Items.Select(r => r.Code).ToList();
            var baseUrl = _baseUrlService.GetBaseUrl().TrimEnd('/');
            
            using var connection = _dapperContext.CreateConnection();
            var replySql = @"
                SELECT r.*, u.""FullName"" as ""UserName"", u.""AvatarUrl"" as ""UserAvatar""
                FROM ""TblReview"" r
                LEFT JOIN ""TblUser"" u ON r.""UserCode"" = u.""Code""
                WHERE r.""ParentCode"" = ANY(@Codes) AND r.""IsActive"" = true
                ORDER BY r.""CreatedAt"" ASC";
                
            var replies = (await SqlMapper.QueryAsync<dynamic>(connection, replySql, new { Codes = parentCodes.ToArray() })).ToList();
            
            var replyMap = replies.GroupBy(r => (string)r.ParentCode).ToDictionary(
                g => g.Key,
                g => g.Select(r => {
                    var avatarPath = (string?)(r.UserAvatar ?? "");
                    var avatarUrl = string.IsNullOrEmpty(avatarPath) ? null : 
                                   (avatarPath.StartsWith("http") ? avatarPath : $"{baseUrl}/{avatarPath.TrimStart('/')}");
                    return new ReviewDto {
                        Code = (string)r.Code,
                        UserCode = (string)r.UserCode,
                        UserName = (string)r.UserName,
                        UserAvatar = avatarUrl,
                        Comment = (string)r.Comment,
                        Rating = (int)(r.Rating ?? 0),
                        CreatedAt = (DateTime?)r.CreatedAt,
                        ParentCode = (string)r.ParentCode
                    };
                }).ToList()
            );

            foreach (var review in result.Value.Items)
            {
                if (replyMap.TryGetValue(review.Code, out var reviewReplies))
                {
                    review.Replies = reviewReplies;
                }
            }

            TransformAvatars(result.Value.Items);
        }

        return result;
    }

    public async Task<Result<IEnumerable<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _repository.AsQueryable()
            .Where(r => r.UserCode == request.UserCode)
            .Include(r => r.UserCodeNavigation)
            .Include(r => r.OrderItemCodeNavigation)
            .ThenInclude(oi => oi!.ProductCodeNavigation)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        TransformAvatars(dtos);
        return Result.Success(dtos);
    }

    public async Task<Result<ReviewDto>> Handle(GetByCodeQuery<ReviewDto> request, CancellationToken cancellationToken)
    {
        var result = await GetByCodeAsync<ReviewDto>(
            request.Code,
            VNVTStore.Application.Common.MessageConstants.Review,
            cancellationToken,
            includes: q => q.Include(r => r.UserCodeNavigation)
                            .Include(r => r.InverseParentNavigation).ThenInclude(rp => rp.UserCodeNavigation));

        if (result.IsSuccess && result.Value != null)
        {
            TransformAvatars(new[] { result.Value });
        }

        return result;
    }

    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
             return Result.Failure(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        review.IsApproved = true;
        _repository.Update(review);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (!string.IsNullOrEmpty(review.ProductCode))
        {
            await UpdateProductRatingAsync(review.ProductCode, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
             return Result.Failure(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        review.IsApproved = false;
        _repository.Update(review);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (!string.IsNullOrEmpty(review.ProductCode))
        {
            await UpdateProductRatingAsync(review.ProductCode, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(ReplyReviewCommand request, CancellationToken cancellationToken)
    {
        var parentReview = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (parentReview == null)
             return Result.Failure(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode))
            return Result.Failure(Error.Unauthorized());

        // Create a new review as a reply
        var replyReview = new TblReview
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10), // Or use sequence logic if possible
            UserCode = userCode,
            ParentCode = parentReview.Code,
            ProductCode = parentReview.ProductCode, // Copy ProductCode from parent
            Comment = request.Reply,
            Rating = 0, // Replies usually don't have separate ratings
            IsApproved = true, // Admin replies or trusted user replies can be auto-approved
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            ModifiedType = "ADD"
        };
        
        await _repository.AddAsync(replyReview, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        var searching = request.Searching ?? new List<SearchDTO>();

        if (request.IsApproved.HasValue)
        {
            searching.Add(new SearchDTO { SearchField = "IsApproved", SearchValue = request.IsApproved.Value, SearchCondition = SearchCondition.Equal });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };
        
        // Since we need complex nested Joins (User and Product), we'll use EF-based search or Dapper with explicit Joins.
        // For reviews, we likely want to see everything in Admin, but the user specifically asked for isActive == true for public APIs.
        
        var result = await GetPagedDapperAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            searching,
            sortDTO,
            new List<ReferenceTable> {
                new ReferenceTable { TableName = "TblUser", AliasName = "UserName", ForeignKeyCol = "UserCode", ColumnName = "FullName" },
                new ReferenceTable { TableName = "TblUser", AliasName = "UserAvatar", ForeignKeyCol = "UserCode", ColumnName = "AvatarUrl" },
                new ReferenceTable { TableName = "TblProduct", AliasName = "ProductName", ForeignKeyCol = "ProductCode", ColumnName = "Name", IsJoinThrough = "OrderItemCodeNavigation" }
            },
            request.Fields,
            cancellationToken);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            TransformAvatars(result.Value.Items);
        }

        return result;
    }
}
