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
    public IActionResult GetTemplate()
    {
        var csv = "Code,Name,Symbol,Description,IsActive\nUNIT001,Kilogram,kg,Weight unit,true";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "units_template.csv");
    }
}
