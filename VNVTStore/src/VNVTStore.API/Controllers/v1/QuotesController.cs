using MediatR;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Quotes.Commands;
using VNVTStore.Application.Quotes.Queries;


namespace VNVTStore.API.Controllers.v1;

[Authorize]
public class QuotesController : BaseApiController
{
    public QuotesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]

    public async Task<ActionResult<ApiResponse<QuoteDto>>> CreateQuote(CreateQuoteCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<QuoteDto>>>> GetMyQuotes()
    {
        return Ok(await Mediator.Send(new GetMyQuotesQuery()));
    }
}


