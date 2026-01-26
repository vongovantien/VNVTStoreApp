using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Categories.Queries;

public record GetCategoryStatsQuery() : IRequest<Result<CategoryStatsDto>>;
