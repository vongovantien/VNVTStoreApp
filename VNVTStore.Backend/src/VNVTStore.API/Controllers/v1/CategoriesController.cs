using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    protected override IRequest<Result<PagedResult<CategoryDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<CategoryDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<CategoryDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<CategoryDto>(code);

    protected override IRequest<Result<CategoryDto>> CreateCreateCommand(CreateCategoryDto dto)
        => new CreateCommand<CreateCategoryDto, CategoryDto>(dto);

    protected override IRequest<Result<CategoryDto>> CreateUpdateCommand(string code, UpdateCategoryDto dto)
        => new UpdateCommand<UpdateCategoryDto, CategoryDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblCategory>(code);

    [HttpPost("delete-multiple")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
    {
        var result = await Mediator.Send(new DeleteMultipleCommand<TblCategory>(codes));
        return HandleDelete(result);
    }
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetCategoryStatsQuery());
        return HandleResult(result);
    }
}
