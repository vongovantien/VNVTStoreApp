using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class TagsController : BaseApiController<TagDto, CreateTagDto, UpdateTagDto>
{
    public TagsController(IMediator mediator) : base(mediator)
    {
    }

    protected override IRequest<Result<PagedResult<TagDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<TagDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<TagDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<TagDto>(code);

    protected override IRequest<Result<TagDto>> CreateCreateCommand(CreateTagDto dto)
        => new CreateCommand<CreateTagDto, TagDto>(dto);

    protected override IRequest<Result<TagDto>> CreateUpdateCommand(string code, UpdateTagDto dto)
        => new UpdateCommand<UpdateTagDto, TagDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblTag>(code);
}
