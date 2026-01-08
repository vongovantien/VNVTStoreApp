using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Process a payment for an order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(
            request.OrderCode, request.PaymentMethod, request.Amount));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Update payment status (Callback/Admin)
    /// </summary>
    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request)
    {
        var result = await _mediator.Send(new UpdatePaymentStatusCommand(
            request.PaymentCode, request.Status, request.TransactionId));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get payment details by order
    /// </summary>
    [HttpGet("order/{orderCode}")]
    public async Task<IActionResult> GetPaymentByOrder(string orderCode)
    {
        var result = await _mediator.Send(new GetPaymentByOrderQuery(orderCode));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get my payment history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _mediator.Send(new GetMyPaymentsQuery());
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }
}

public record ProcessPaymentRequest(string OrderCode, string PaymentMethod, decimal Amount);
public record UpdatePaymentStatusRequest(string PaymentCode, string Status, string? TransactionId);
