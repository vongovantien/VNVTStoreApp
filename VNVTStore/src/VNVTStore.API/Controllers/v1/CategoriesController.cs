using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Categories.Queries;

using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : BaseApiController<CategoryDto, CreateCategoryDto, UpdateCategoryDto>
{
    public CategoriesController(IMediator mediator) : base(mediator)
    {
    }

    protected override IRequest<Result<PagedResult<CategoryDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort)
        => new GetPagedQuery<CategoryDto>(pageIndex, pageSize, search, sort);

    protected override IRequest<Result<CategoryDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<CategoryDto>(code);

    protected override IRequest<Result<CategoryDto>> CreateCreateCommand(CreateCategoryDto dto)
        => new CreateCommand<CreateCategoryDto, CategoryDto>(dto);

    protected override IRequest<Result<CategoryDto>> CreateUpdateCommand(string code, UpdateCategoryDto dto)
        => new UpdateCommand<UpdateCategoryDto, CategoryDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblCategory>(code);
}
