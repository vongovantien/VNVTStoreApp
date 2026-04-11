using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class SuppliersController : BaseApiController<TblSupplier, SupplierDto, CreateSupplierDto, UpdateSupplierDto>
{
    public SuppliersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("delete-multiple")]
    public override async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
    {
        var result = await Mediator.Send(new DeleteMultipleCommand<TblSupplier>(codes));
        return HandleDelete(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetStatsQuery<TblSupplier>());
        return HandleResult(result);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public override async Task<IActionResult> GetTemplate()
    {
        var bytes = await Task.Run(() => VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.SupplierImportDto>());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "suppliers_template.xlsx");
    }
}
