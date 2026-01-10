using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
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
    IRequestHandler<GetPagedQuery<ReviewDto>, Result<PagedResult<ReviewDto>>>
{
    private readonly IRepository<TblOrderItem> _orderItemRepository;
    private readonly ICurrentUser _currentUser;

    public ReviewHandlers(
        IRepository<TblReview> reviewRepository,
        IRepository<TblOrderItem> orderItemRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(reviewRepository, unitOfWork, mapper)
    {
        _orderItemRepository = orderItemRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ReviewDto>> Handle(CreateCommand<CreateReviewDto, ReviewDto> request, CancellationToken cancellationToken)
    {
        // Validate order item exists and belongs to user
        var orderItem = await _orderItemRepository.AsQueryable()
            .Include(oi => oi.OrderCodeNavigation)
            .FirstOrDefaultAsync(oi => oi.Code == request.Dto.OrderItemCode, cancellationToken);

        if (orderItem == null)
            return Result.Failure<ReviewDto>(Error.NotFound(MessageConstants.OrderItem, request.Dto.OrderItemCode));

        if (orderItem.OrderCodeNavigation.UserCode != request.Dto.UserCode)
            return Result.Failure<ReviewDto>(Error.Forbidden("Cannot review item from another user's order"));

        // Check if already reviewed
        var existingReview = await Repository.FindAsync(
            r => r.OrderItemCode == request.Dto.OrderItemCode && r.UserCode == request.Dto.UserCode,
            cancellationToken);

        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Conflict(MessageConstants.ReviewAlreadyExists));

        return await CreateAsync<CreateReviewDto, ReviewDto>(
            request.Dto,
            cancellationToken,
            r => {
                r.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                r.IsApproved = false; // Changed to false for moderation
                r.CreatedAt = DateTime.Now;
            });
    }

    public async Task<Result<ReviewDto>> Handle(UpdateCommand<UpdateReviewDto, ReviewDto> request, CancellationToken cancellationToken)
    {
        var review = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
            return Result.Failure<ReviewDto>(Error.NotFound(MessageConstants.Review, request.Code));

        var userCode = _currentUser.UserCode;
        if (review.UserCode != userCode)
            return Result.Failure<ReviewDto>(Error.Forbidden("Cannot update another user's review"));

        return await UpdateAsync<UpdateReviewDto, ReviewDto>(
            request.Code,
            request.Dto,
            MessageConstants.Review,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblReview> request, CancellationToken cancellationToken)
    {
        var review = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
            return Result.Failure(Error.NotFound(MessageConstants.Review, request.Code));

        var userCode = _currentUser.UserCode;
        var isAdmin = _currentUser.IsAdmin;

        if (review.UserCode != userCode && !isAdmin)
            return Result.Failure(Error.Forbidden("Cannot delete another user's review"));

        return await DeleteAsync(request.Code, MessageConstants.Review, cancellationToken, softDelete: false);
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: r => r.OrderItemCodeNavigation != null && 
                            r.OrderItemCodeNavigation.ProductCode == request.ProductCode &&
                            r.IsApproved == true,
            includes: q => q.Include(r => r.UserCodeNavigation).Include(r => r.OrderItemCodeNavigation),
            orderBy: q => q.OrderByDescending(r => r.CreatedAt));
    }

    public async Task<Result<IEnumerable<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await Repository.AsQueryable()
            .Where(r => r.UserCode == request.UserCode)
            .Include(r => r.OrderItemCodeNavigation)
            .ThenInclude(oi => oi!.ProductCodeNavigation)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(Mapper.Map<IEnumerable<ReviewDto>>(reviews));
    }

    public async Task<Result<ReviewDto>> Handle(GetByCodeQuery<ReviewDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<ReviewDto>(
            request.Code,
            MessageConstants.Review,
            cancellationToken,
            includes: q => q.Include(r => r.UserCodeNavigation));
    }

    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
             return Result.Failure(Error.NotFound(MessageConstants.Review, request.Code));

        review.IsApproved = true;
        Repository.Update(review);
        await UnitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (review == null)
             return Result.Failure(Error.NotFound(MessageConstants.Review, request.Code));

        review.IsApproved = false;
        Repository.Update(review);
        await UnitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetPagedQuery<ReviewDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<ReviewDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: r => true, // Review GetAllReviewsQuery used IsApproved. Generic GetPagedQuery doesn't have it.
            includes: q => q.Include(r => r.UserCodeNavigation)
                             .Include(r => r.OrderItemCodeNavigation)
                             .ThenInclude(oi => oi!.ProductCodeNavigation),
            orderBy: q => q.OrderByDescending(r => r.CreatedAt));
    }
}
