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
    public IActionResult GetTemplate()
    {
        var csv = "Code,Name,Email,Phone,Address,TaxCode,ContactPerson,IsActive\nSUP001,Sample Supplier,supplier@example.com,0901234567,123 Street,123456789,John Doe,true";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", "suppliers_template.csv");
    }
}
