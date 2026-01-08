using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Promotions.Commands;

public record CreatePromotionCommand(
    string Name,
    string? Description,
    string DiscountType, // "percentage" or "fixed"
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit
) : IRequest<Result<PromotionDto>>;

public record UpdatePromotionCommand(
    string PromotionCode,
    string? Name,
    string? Description,
    decimal? DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    int? UsageLimit,
    bool? IsActive
) : IRequest<Result<PromotionDto>>;

public record DeletePromotionCommand(string PromotionCode) : IRequest<Result<bool>>;
