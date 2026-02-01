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
    IRequestHandler<GetAllReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    private readonly IRepository<TblOrderItem> _orderItemRepository;
    private readonly ICurrentUser _currentUser;

    public ReviewHandlers(
        IRepository<TblReview> reviewRepository,
        IRepository<TblOrderItem> orderItemRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(reviewRepository, unitOfWork, mapper, dapperContext)
    {
        _orderItemRepository = orderItemRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ReviewDto>> Handle(CreateCommand<CreateReviewDto, ReviewDto> request, CancellationToken cancellationToken)
    {
        string? productCode = request.Dto.ProductCode;
        
        // If order item is provided, validate and link
        if (!string.IsNullOrEmpty(request.Dto.OrderItemCode))
        {
            var orderItem = await _orderItemRepository.AsQueryable()
                .Include(oi => oi.OrderCodeNavigation)
                .FirstOrDefaultAsync(oi => oi.Code == request.Dto.OrderItemCode, cancellationToken);

            if (orderItem == null)
                return Result.Failure<ReviewDto>(Error.NotFound(VNVTStore.Application.Common.MessageConstants.OrderItem, request.Dto.OrderItemCode));

            if (orderItem.OrderCodeNavigation != null && orderItem.OrderCodeNavigation.UserCode != request.Dto.UserCode)
            {
                return Result.Failure<ReviewDto>(Error.Forbidden(VNVTStore.Application.Common.MessageConstants.Forbidden));
            }
            
            productCode = orderItem.ProductCode;
        }

        // Check if already reviewed (by Either ProductCode or OrderItemCode)
        var existingReview = await _repository.FindAsync(
            r => (request.Dto.OrderItemCode != null && r.OrderItemCode == request.Dto.OrderItemCode) || 
                 (productCode != null && r.ProductCode == productCode && r.UserCode == request.Dto.UserCode),
            cancellationToken);

        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Conflict(VNVTStore.Application.Common.MessageConstants.ReviewAlreadyExists));

        return await CreateAsync<CreateReviewDto, ReviewDto>(
            request.Dto,
            cancellationToken,
            r => {
                r.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                r.IsApproved = false; 
                r.CreatedAt = DateTime.Now;
                r.ProductCode = productCode;
            });
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

        if (review.UserCode != userCode && !isAdmin)
            return Result.Failure(Error.Forbidden("Cannot delete another user's review"));

        return await DeleteAsync(request.Code, VNVTStore.Application.Common.MessageConstants.Review, cancellationToken, softDelete: false);
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: r => ((r.OrderItemCodeNavigation != null && r.OrderItemCodeNavigation.ProductCode == request.ProductCode) || 
                             r.ProductCode == request.ProductCode) &&
                            r.IsApproved == true,
            includes: q => q.Include(r => r.UserCodeNavigation)
                            .Include(r => r.OrderItemCodeNavigation)
                            .ThenInclude(oi => oi!.ProductCodeNavigation),
            orderBy: q => q.OrderByDescending(r => r.CreatedAt));
    }

    public async Task<Result<IEnumerable<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _repository.AsQueryable()
            .Where(r => r.UserCode == request.UserCode)
            .Include(r => r.OrderItemCodeNavigation)
            .ThenInclude(oi => oi!.ProductCodeNavigation)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<ReviewDto>>(reviews));
    }

    public async Task<Result<ReviewDto>> Handle(GetByCodeQuery<ReviewDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<ReviewDto>(
            request.Code,
            VNVTStore.Application.Common.MessageConstants.Review,
            cancellationToken,
            includes: q => q.Include(r => r.UserCodeNavigation));
    }

    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
             return Result.Failure(Error.NotFound(VNVTStore.Application.Common.MessageConstants.Review, request.Code));

        review.IsApproved = true;
        _repository.Update(review);
        await _unitOfWork.CommitAsync(cancellationToken);

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

        return Result.Success();
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();

        if (!string.IsNullOrEmpty(request.Search))
        {
            // Search across Comment, User FullName, and Product Name
            searchFields.Add(new SearchDTO { SearchField = "Comment", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
            searchFields.Add(new SearchDTO { SearchField = "UserCodeNavigation.FullName", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
            searchFields.Add(new SearchDTO { SearchField = "OrderItemCodeNavigation.ProductCodeNavigation.Name", SearchValue = request.Search, SearchCondition = SearchCondition.Contains, GroupID = 1, CombineCondition = "OR" });
        }

        if (request.IsApproved.HasValue)
        {
            searchFields.Add(new SearchDTO { SearchField = "IsApproved", SearchValue = request.IsApproved.Value, SearchCondition = SearchCondition.Equal });
        }

        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };
        
        // Since we need complex nested Joins (User and Product), we'll use EF-based search or Dapper with explicit Joins.
        // For reviews, we likely want to see everything in Admin, but the user specifically asked for isActive == true for public APIs.
        
        return await GetPagedDapperAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            searchFields,
            sortDTO,
            new List<ReferenceTable> {
                new ReferenceTable { TableName = "TblUser", AliasName = "User", ForeignKeyCol = "UserCode", ColumnName = "FullName" },
                new ReferenceTable { TableName = "TblProduct", AliasName = "Product", ForeignKeyCol = "ProductCode", ColumnName = "Name", IsJoinThrough = "OrderItemCodeNavigation" }
            },
            request.Fields,
            cancellationToken);
    }
}
