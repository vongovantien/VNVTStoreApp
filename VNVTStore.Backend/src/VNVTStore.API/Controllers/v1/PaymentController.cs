using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;
using VNVTStore.Domain.Enums;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class PaymentController : BaseApiController
{
    public PaymentController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await Mediator.Send(new GetMyPaymentsQuery());
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAllPayments([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllPaymentsQuery(pageIndex, pageSize));
        return HandleResult(result);
    }

    [HttpPost("status")]
    // [Authorize(Roles = nameof(UserRole.Admin))] // Or system generated
    public async Task<IActionResult> UpdatePaymentStatus([FromBody] UpdatePaymentStatusCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
