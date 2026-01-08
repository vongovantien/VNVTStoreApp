using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Application.Reviews.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Reviews.Handlers;

public class ReviewHandlers :
    IRequestHandler<CreateReviewCommand, Result<ReviewDto>>,
    IRequestHandler<UpdateReviewCommand, Result<ReviewDto>>,
    IRequestHandler<DeleteReviewCommand, Result<bool>>,
    IRequestHandler<GetProductReviewsQuery, Result<PagedResult<ReviewDto>>>,
    IRequestHandler<GetUserReviewsQuery, Result<IEnumerable<ReviewDto>>>,
    IRequestHandler<GetReviewByCodeQuery, Result<ReviewDto>>
{
    private readonly IRepository<TblReview> _reviewRepository;
    private readonly IRepository<TblOrderItem> _orderItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewHandlers(
        IRepository<TblReview> reviewRepository,
        IRepository<TblOrderItem> orderItemRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _orderItemRepository = orderItemRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Validate order item exists and belongs to user
        var orderItem = await _orderItemRepository.AsQueryable()
            .Include(oi => oi.OrderCodeNavigation)
            .FirstOrDefaultAsync(oi => oi.Code == request.OrderItemCode, cancellationToken);

        if (orderItem == null)
            return Result.Failure<ReviewDto>(Error.NotFound("OrderItem", request.OrderItemCode));

        if (orderItem.OrderCodeNavigation.UserCode != request.UserCode)
            return Result.Failure<ReviewDto>(Error.Forbidden("Cannot review item from another user's order"));

        // Check if already reviewed
        var existingReview = await _reviewRepository.FindAsync(
            r => r.OrderItemCode == request.OrderItemCode && r.UserCode == request.UserCode,
            cancellationToken);

        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Conflict("You have already reviewed this item"));

        var review = new TblReview
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = request.UserCode,
            OrderItemCode = request.OrderItemCode,
            Rating = request.Rating,
            Comment = request.Content ?? request.Title, // Combine title and content into Comment
            IsApproved = true, // Auto-approve for now
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }

    public async Task<Result<ReviewDto>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByCodeAsync(request.ReviewCode, cancellationToken);

        if (review == null)
            return Result.Failure<ReviewDto>(Error.NotFound("Review", request.ReviewCode));

        if (review.UserCode != request.UserCode)
            return Result.Failure<ReviewDto>(Error.Forbidden("Cannot update another user's review"));

        if (request.Rating.HasValue) review.Rating = request.Rating.Value;
        if (request.Content != null || request.Title != null) 
            review.Comment = request.Content ?? request.Title;

        _reviewRepository.Update(review);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }

    public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByCodeAsync(request.ReviewCode, cancellationToken);

        if (review == null)
            return Result.Failure<bool>(Error.NotFound("Review", request.ReviewCode));

        if (review.UserCode != request.UserCode)
            return Result.Failure<bool>(Error.Forbidden("Cannot delete another user's review"));

        _reviewRepository.Delete(review);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _reviewRepository.AsQueryable()
            .Include(r => r.UserCodeNavigation)
            .Include(r => r.OrderItemCodeNavigation)
            .Where(r => r.OrderItemCodeNavigation != null && 
                        r.OrderItemCodeNavigation.ProductCode == request.ProductCode &&
                        r.IsApproved == true);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ReviewDto>>(items);
        return Result.Success(new PagedResult<ReviewDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<IEnumerable<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.AsQueryable()
            .Where(r => r.UserCode == request.UserCode)
            .Include(r => r.OrderItemCodeNavigation)
            .ThenInclude(oi => oi!.ProductCodeNavigation)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<IEnumerable<ReviewDto>>(reviews));
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByCodeQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.AsQueryable()
            .Include(r => r.UserCodeNavigation)
            .FirstOrDefaultAsync(r => r.Code == request.ReviewCode, cancellationToken);

        if (review == null)
            return Result.Failure<ReviewDto>(Error.NotFound("Review", request.ReviewCode));

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}
