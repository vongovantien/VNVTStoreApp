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

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var csv = "Code,Name,Description,ImageUrl,IsActive\nBRAND001,Sample Brand,Brand description,,true";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "brands_template.csv");
    }
}
