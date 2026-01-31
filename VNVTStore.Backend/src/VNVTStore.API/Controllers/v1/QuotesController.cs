using MediatR;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;


namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[Authorize]
public class QuotesController : BaseApiController<TblQuote, QuoteDto, CreateQuoteDto, UpdateQuoteDto>
{
    public QuotesController(IMediator mediator) : base(mediator)
    {
    }

    // Use standard Create from base
    [HttpPost]
    [AllowAnonymous]
    public override Task<IActionResult> Create([FromBody] RequestDTO<CreateQuoteDto> request) => base.Create(request);

    [HttpGet]
    public async Task<IActionResult> GetMyQuotes()
    {
        var result = await Mediator.Send(new GetAllQuery<QuoteDto>());
        return HandleResult(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetStatsQuery<TblQuote>());
        return HandleResult(result);
    }
}


