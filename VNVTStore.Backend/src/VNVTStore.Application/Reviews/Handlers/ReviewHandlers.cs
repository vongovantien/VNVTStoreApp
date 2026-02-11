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
    IRequestHandler<ReplyReviewCommand, Result>,
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
        string? productCode = string.IsNullOrEmpty(request.Dto.ProductCode) ? null : request.Dto.ProductCode;
        string? orderItemCode = string.IsNullOrEmpty(request.Dto.OrderItemCode) ? null : request.Dto.OrderItemCode;
        
        // If order item is provided, validate and link
        if (!string.IsNullOrEmpty(orderItemCode))
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
            r => (orderItemCode != null && r.OrderItemCode == orderItemCode) || 
                 (productCode != null && r.ProductCode == productCode && r.UserCode == request.Dto.UserCode),
            cancellationToken);

        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Conflict(VNVTStore.Application.Common.MessageConstants.ReviewAlreadyExists));

        return await CreateAsync<CreateReviewDto, ReviewDto>(
            request.Dto,
            cancellationToken,
            r => {
                r.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                r.IsApproved = true; // Auto-approve
                r.CreatedAt = DateTime.Now;
                r.ProductCode = productCode;
                r.OrderItemCode = orderItemCode;
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
        var searchFields = new List<SearchDTO>
        {
            new SearchDTO { SearchField = "ProductCode", SearchValue = request.ProductCode, SearchCondition = SearchCondition.Equal },
            new SearchDTO { SearchField = "ParentCode", SearchValue = null, SearchCondition = SearchCondition.IsNull }
        };

        return await GetPagedDapperAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            searchFields,
            null, // Sort
            new List<ReferenceTable> {
                new ReferenceTable { TableName = "TblUser", AliasName = "User", ForeignKeyCol = "UserCode", ColumnName = "FullName" },
                new ReferenceTable { TableName = "TblProduct", AliasName = "Product", ForeignKeyCol = "ProductCode", ColumnName = "Name", IsJoinThrough = "OrderItemCodeNavigation" }
            },
            null, // Fields
            cancellationToken);
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
            includes: q => q.Include(r => r.UserCodeNavigation)
                            .Include(r => r.InverseParentNavigation).ThenInclude(rp => rp.UserCodeNavigation));
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
        
        return await GetPagedDapperAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            searching,
            sortDTO,
            new List<ReferenceTable> {
                new ReferenceTable { TableName = "TblUser", AliasName = "User", ForeignKeyCol = "UserCode", ColumnName = "FullName" },
                new ReferenceTable { TableName = "TblProduct", AliasName = "Product", ForeignKeyCol = "ProductCode", ColumnName = "Name", IsJoinThrough = "OrderItemCodeNavigation" }
            },
            request.Fields,
            cancellationToken);
    }
}
