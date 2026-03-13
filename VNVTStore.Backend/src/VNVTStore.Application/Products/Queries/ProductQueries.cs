using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Queries;




public record GetProductByCodeQuery(string Code, bool IncludeChildren = false) : GetByCodeQuery<ProductDto>(Code, IncludeChildren);

public record GetProductStatsQuery() : IRequest<Result<ProductStatsDto>>;
public record GetTrendingProductsQuery(int Limit = 8) : IRequest<Result<List<ProductDto>>>;

public record GetRelatedProductsQuery(string ProductCode, int Limit = 10) : IRequest<Result<List<ProductDto>>>;

public record GetProductQuestionsQuery(string ProductCode) : IRequest<Result<List<ReviewDto>>>;
