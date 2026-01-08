using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Reviews.Commands;

public record CreateReviewCommand(
    string UserCode,
    string OrderItemCode,
    int Rating,
    string? Title,
    string? Content
) : IRequest<Result<ReviewDto>>;

public record UpdateReviewCommand(
    string ReviewCode,
    string UserCode,
    int? Rating,
    string? Title,
    string? Content
) : IRequest<Result<ReviewDto>>;

public record DeleteReviewCommand(string ReviewCode, string UserCode) : IRequest<Result<bool>>;

public record MarkReviewHelpfulCommand(string ReviewCode) : IRequest<Result<bool>>;
