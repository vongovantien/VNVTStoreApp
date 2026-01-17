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

    [HttpPost]
    [AllowAnonymous]
    public override async Task<IActionResult> Create([FromBody] RequestDTO<CreateQuoteDto> request)
    {
        if (request.PostObject == null)
            return BadRequest(new { message = MessageConstants.Get(MessageConstants.BadRequest) });
            
        var dto = request.PostObject;
        var command = new Application.Quotes.Commands.CreateQuoteCommand
        {
            ProductCode = dto.ProductCode,
            Quantity = dto.Quantity,
            Note = dto.Note,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone
        };
        var result = await Mediator.Send(command);
        
        if (result.Success) return Ok(result.Data);
        return BadRequest(new { message = result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuotes()
    {
        var result = await Mediator.Send(new GetAllQuery<QuoteDto>());
        return HandleResult(result);
    }

    // Abstract methods implementation
    protected override IRequest<Result<PagedResult<QuoteDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters, List<string>? fields = null)
        => new GetPagedQuery<QuoteDto>(pageIndex, pageSize, search, sort, filters, fields);

    protected override IRequest<Result<QuoteDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<QuoteDto>(code);

    protected override IRequest<Result<QuoteDto>> CreateCreateCommand(CreateQuoteDto dto)
        => new CreateCommand<CreateQuoteDto, QuoteDto>(dto);

    protected override IRequest<Result<QuoteDto>> CreateUpdateCommand(string code, UpdateQuoteDto dto)
        => new UpdateCommand<UpdateQuoteDto, QuoteDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblQuote>(code);
}


