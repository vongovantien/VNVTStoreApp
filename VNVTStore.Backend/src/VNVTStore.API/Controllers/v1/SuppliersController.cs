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
[Authorize(Roles = "admin,Admin")]
public class SuppliersController : BaseApiController<SupplierDto, CreateSupplierDto, UpdateSupplierDto>
{
    public SuppliersController(IMediator mediator) : base(mediator)
    {
    }

    protected override IRequest<Result<PagedResult<SupplierDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<SupplierDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<SupplierDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<SupplierDto>(code);

    protected override IRequest<Result<SupplierDto>> CreateCreateCommand(CreateSupplierDto dto)
        => new CreateCommand<CreateSupplierDto, SupplierDto>(dto);

    protected override IRequest<Result<SupplierDto>> CreateUpdateCommand(string code, UpdateSupplierDto dto)
        => new UpdateCommand<UpdateSupplierDto, SupplierDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblSupplier>(code);

    [HttpPost("delete-multiple")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
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
}
