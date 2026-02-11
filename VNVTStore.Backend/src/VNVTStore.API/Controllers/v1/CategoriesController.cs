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

    [HttpPost("import")]
    [Authorize(Roles = "admin,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var result = await Mediator.Send(new VNVTStore.Application.Categories.Commands.ImportCategoriesCommand(memoryStream));
        return HandleResult(result);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var bytes = VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.CategoryImportDto>();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "categories_template.xlsx");
    }
}
