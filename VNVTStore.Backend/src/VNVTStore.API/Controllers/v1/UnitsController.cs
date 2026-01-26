using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class UnitsController : BaseApiController<CatalogUnitDto, CreateCatalogUnitDto, UpdateCatalogUnitDto>
{
    public UnitsController(IMediator mediator) : base(mediator)
    {
    }

    protected override IRequest<Result<PagedResult<CatalogUnitDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<CatalogUnitDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<CatalogUnitDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<CatalogUnitDto>(code);

    protected override IRequest<Result<CatalogUnitDto>> CreateCreateCommand(CreateCatalogUnitDto dto)
        => new CreateCommand<CreateCatalogUnitDto, CatalogUnitDto>(dto);

    protected override IRequest<Result<CatalogUnitDto>> CreateUpdateCommand(string code, UpdateCatalogUnitDto dto)
        => new UpdateCommand<UpdateCatalogUnitDto, CatalogUnitDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblUnit>(code);
}
