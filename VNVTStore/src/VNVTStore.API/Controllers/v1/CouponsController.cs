using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Validate a coupon code
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var result = await _mediator.Send(new ValidateCouponCommand(request.CouponCode, request.OrderAmount));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get coupon details by code
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetCoupon(string code)
    {
        var result = await _mediator.Send(new GetCouponByCodeQuery(code));
        if (result.IsFailure) return NotFound(result.Error);
        return Ok(result.Value);
    }
}

public record ValidateCouponRequest(string CouponCode, decimal OrderAmount);
