using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

using VNVTStore.Application.Suppliers.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class SuppliersController : BaseApiController<SupplierDto, CreateSupplierDto, UpdateSupplierDto>
{
    public SuppliersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers(
        [FromQuery] int pageIndex = AppConstants.Paging.DefaultPageNumber,
        [FromQuery] int pageSize = AppConstants.Paging.DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await Mediator.Send(new GetAllSuppliersQuery(pageIndex, pageSize, search, isActive));
        return HandleResult(result);
    }

    protected override IRequest<Result<PagedResult<SupplierDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters)
        => new GetPagedQuery<SupplierDto>(pageIndex, pageSize, search, sort, filters);

    protected override IRequest<Result<SupplierDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<SupplierDto>(code);

    protected override IRequest<Result<SupplierDto>> CreateCreateCommand(CreateSupplierDto dto)
        => new CreateCommand<CreateSupplierDto, SupplierDto>(dto);

    protected override IRequest<Result<SupplierDto>> CreateUpdateCommand(string code, UpdateSupplierDto dto)
        => new UpdateCommand<UpdateSupplierDto, SupplierDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblSupplier>(code);
}
