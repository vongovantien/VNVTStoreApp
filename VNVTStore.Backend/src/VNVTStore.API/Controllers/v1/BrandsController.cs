using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class BrandsController : BaseApiController<BrandDto, CreateBrandDto, UpdateBrandDto>
{
    public BrandsController(IMediator mediator) : base(mediator)
    {
    }

    // BaseApiController handles standard CRUD via Search, Get, Create, Update, Delete methods
    // We only need to provide the factory methods below.


    protected override IRequest<Result<PagedResult<BrandDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<BrandDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<BrandDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<BrandDto>(code);

    protected override IRequest<Result<BrandDto>> CreateCreateCommand(CreateBrandDto dto)
        => new CreateCommand<CreateBrandDto, BrandDto>(dto);

    protected override IRequest<Result<BrandDto>> CreateUpdateCommand(string code, UpdateBrandDto dto)
        => new UpdateCommand<UpdateBrandDto, BrandDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblBrand>(code);

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetStatsQuery<TblBrand>());
        return HandleResult(result);
    }
}
