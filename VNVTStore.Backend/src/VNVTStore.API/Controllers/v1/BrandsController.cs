using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class BrandsController : BaseApiController<TblBrand, BrandDto, CreateBrandDto, UpdateBrandDto>
{
    public BrandsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetStatsQuery<TblBrand>());
        return HandleResult(result);
    }

    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Consumes("multipart/form-data")]
    public override async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var result = await Mediator.Send(new VNVTStore.Application.Brands.Commands.ImportBrandsCommand(memoryStream));
        return HandleResult(result);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public override async Task<IActionResult> GetTemplate()
    {
        var bytes = await Task.Run(() => VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.BrandImportDto>());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "brands_template.xlsx");
    }
}
