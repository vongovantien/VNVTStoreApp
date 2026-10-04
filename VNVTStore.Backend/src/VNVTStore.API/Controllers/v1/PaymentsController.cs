using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Payments.Commands;
using VNVTStore.Application.Payments.Queries;
using VNVTStore.Domain.Enums;

namespace VNVTStore.API.Controllers.v1;

[Authorize]
public class PaymentsController : BaseApiController
{
    public PaymentsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Process a payment record for an order (COD, BankTransfer, or initial record)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
        {
            return BadRequest(ApiResponse<string>.Fail("Invalid payment method."));
        }

        var result = await Mediator.Send(new ProcessPaymentCommand(request.OrderCode, method));
        return HandleResult(result);
    }

    /// <summary>
    /// Generate an online payment URL (VNPay, MoMo) for an order
    /// </summary>
    [HttpPost("{orderCode}/checkout")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCheckoutUrl(string orderCode, [FromBody] CheckoutUrlRequest? request)
    {
        PaymentMethod? method = null;
        if (!string.IsNullOrEmpty(request?.PaymentMethod) &&
            Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var parsedMethod))
        {
            method = parsedMethod;
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var result = await Mediator.Send(new CreatePaymentUrlCommand(orderCode, clientIp, method));
        return HandleResult(result);
    }

    /// <summary>
    /// Update payment status (Admin only)
    /// </summary>
    [HttpPost("status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request)
    {
        if (!Enum.TryParse<PaymentStatus>(request.Status, true, out var status))
        {
            return BadRequest(ApiResponse<string>.Fail("Invalid payment status."));
        }

        var result = await Mediator.Send(new UpdatePaymentStatusCommand(
            request.PaymentCode, status, request.TransactionId));
        
        return HandleResult(result);
    }

    /// <summary>
    /// Get payment details by order
    /// </summary>
    [HttpGet("order/{orderCode}")]
    [AllowAnonymous]
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

    /// <summary>
    /// Get all payments with paging (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAllPayments([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllPaymentsQuery(pageIndex, pageSize));
        return HandleResult(result);
    }

    /// <summary>
    /// VNPay Return URL callback endpoint (for browser redirect)
    /// </summary>
    [HttpGet("vnpay/return")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayReturn()
    {
        var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
        var result = await Mediator.Send(new ConfirmGatewayPaymentCommand(PaymentMethod.VnPay, dict));
        return HandleResult(result);
    }

    /// <summary>
    /// VNPay IPN webhook (server-to-server)
    /// Returns VNPay standard JSON response
    /// </summary>
    [HttpGet("vnpay/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayIpn()
    {
        var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
        var result = await Mediator.Send(new ConfirmGatewayPaymentCommand(PaymentMethod.VnPay, dict));

        if (result.IsFailure)
        {
            return Ok(new { RspCode = "99", Message = "Unknown error" });
        }

        return result.Value.Outcome switch
        {
            PaymentConfirmOutcome.Confirmed or PaymentConfirmOutcome.AlreadyConfirmed
                => Ok(new { RspCode = "00", Message = "Confirm Success" }),
            PaymentConfirmOutcome.InvalidSignature
                => Ok(new { RspCode = "97", Message = "Invalid Checksum" }),
            PaymentConfirmOutcome.NotFound
                => Ok(new { RspCode = "01", Message = "Order not found" }),
            PaymentConfirmOutcome.InvalidAmount
                => Ok(new { RspCode = "04", Message = "Invalid amount" }),
            _ => Ok(new { RspCode = "00", Message = "Confirm Success" })
        };
    }

    /// <summary>
    /// MoMo Return URL callback endpoint (browser redirect)
    /// </summary>
    [HttpGet("momo/return")]
    [HttpPost("momo/return")]
    [AllowAnonymous]
    public async Task<IActionResult> MoMoReturn()
    {
        Dictionary<string, string> dict;
        if (Request.Method == "POST" && Request.HasFormContentType)
        {
            dict = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
        }
        else
        {
            dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
        }

        var result = await Mediator.Send(new ConfirmGatewayPaymentCommand(PaymentMethod.MoMo, dict));
        return HandleResult(result);
    }

    /// <summary>
    /// MoMo IPN webhook (server-to-server)
    /// </summary>
    [HttpPost("momo/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> MoMoIpn([FromBody] Dictionary<string, object> body)
    {
        var dict = body.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "");
        var result = await Mediator.Send(new ConfirmGatewayPaymentCommand(PaymentMethod.MoMo, dict));

        if (result.IsFailure || !result.Value.IsSuccess)
        {
            return NoContent();
        }

        return NoContent();
    }
}

public record ProcessPaymentRequest(string OrderCode, string PaymentMethod);
public record CheckoutUrlRequest(string? PaymentMethod);
public record UpdatePaymentStatusRequest(string PaymentCode, string Status, string? TransactionId);
