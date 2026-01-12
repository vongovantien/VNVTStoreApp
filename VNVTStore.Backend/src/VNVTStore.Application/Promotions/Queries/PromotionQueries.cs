using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Promotions.Queries;

public record GetFlashSaleQuery() : IRequest<Result<List<PromotionDto>>>;
