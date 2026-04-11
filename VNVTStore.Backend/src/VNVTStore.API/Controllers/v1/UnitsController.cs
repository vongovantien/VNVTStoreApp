using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace VNVTStore.API.Controllers.v1;

public class UnitsController : BaseApiController<TblUnit, CatalogUnitDto, CreateCatalogUnitDto, UpdateCatalogUnitDto>
{
    public UnitsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public override async Task<IActionResult> GetTemplate()
    {
        var bytes = await Task.Run(() => VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.UnitImportDto>());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "units_template.xlsx");
    }
}
