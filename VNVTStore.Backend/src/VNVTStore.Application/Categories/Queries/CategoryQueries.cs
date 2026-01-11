using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Categories.Queries;

public record GetCategoriesQuery(RequestDTO Request) : IRequest<Result<PagedResult<CategoryDto>>>;

public record GetCategoryByCodeQuery(string Code) : GetByCodeQuery<CategoryDto>(Code);
