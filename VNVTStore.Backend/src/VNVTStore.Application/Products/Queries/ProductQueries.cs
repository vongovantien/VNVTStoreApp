using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Products.Queries;




public record GetProductByCodeQuery(string Code) : GetByCodeQuery<ProductDto>(Code);

public record GetProductStatsQuery() : IRequest<Result<ProductStatsDto>>;
