using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;

namespace VNVTStore.API.Controllers.v1;

[Authorize]
public class PaymentsController : BaseApiController
{
    public PaymentsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Process a payment for an order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var result = await Mediator.Send(new ProcessPaymentCommand(
            request.OrderCode, request.PaymentMethod, request.Amount));
        
        return HandleResult(result);
    }

    /// <summary>
    /// Update payment status (Callback/Admin)
    /// </summary>
    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request)
    {
        var result = await Mediator.Send(new UpdatePaymentStatusCommand(
            request.PaymentCode, request.Status, request.TransactionId));
        
        return HandleResult(result);
    }

    /// <summary>
    /// Get payment details by order
    /// </summary>
    [HttpGet("order/{orderCode}")]
    public async Task<IActionResult> GetPaymentByOrder(string orderCode)
    {
        var result = await Mediator.Send(new GetPaymentByOrderQuery(orderCode));
        return HandleResult(result);
    }

    /// <summary>
    /// Get my payment history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await Mediator.Send(new GetMyPaymentsQuery());
        return HandleResult(result);
    }
}

public record ProcessPaymentRequest(string OrderCode, string PaymentMethod, decimal Amount);
public record UpdatePaymentStatusRequest(string PaymentCode, string Status, string? TransactionId);
