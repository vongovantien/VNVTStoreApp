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
public class CategoriesController : BaseApiController<TblCategory, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
{
    public CategoriesController(IMediator mediator) : base(mediator)
    {
    }

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

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var csv = "Code,Name,Description,ParentCategoryCode,IsActive\nCAT001,Sample Category,Description here,,true";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "categories_template.csv");
    }
}
