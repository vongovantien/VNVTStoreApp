using MediatR;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;


namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[Authorize]
public class QuotesController : BaseApiController<QuoteDto, CreateQuoteDto, UpdateQuoteDto>
{
    public QuotesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuotes()
    {
        var result = await Mediator.Send(new GetAllQuery<QuoteDto>());
        return HandleResult(result);
    }

    // Abstract methods implementation
    protected override IRequest<Result<PagedResult<QuoteDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters)
        => new GetPagedQuery<QuoteDto>(pageIndex, pageSize, search, sort, filters);

    protected override IRequest<Result<QuoteDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<QuoteDto>(code);

    protected override IRequest<Result<QuoteDto>> CreateCreateCommand(CreateQuoteDto dto)
        => new CreateCommand<CreateQuoteDto, QuoteDto>(dto);

    protected override IRequest<Result<QuoteDto>> CreateUpdateCommand(string code, UpdateQuoteDto dto)
        => new UpdateCommand<UpdateQuoteDto, QuoteDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblQuote>(code);
}


