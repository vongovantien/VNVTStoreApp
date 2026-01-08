using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Promotions.Commands;
using VNVTStore.Application.Promotions.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PromotionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get active promotions (Public)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActivePromotions()
    {
        var result = await _mediator.Send(new GetActivePromotionsQuery());
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get all promotions (Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllPromotions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetAllPromotionsQuery(pageIndex, pageSize, isActive));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get promotion by code
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetPromotion(string code)
    {
        var result = await _mediator.Send(new GetPromotionByCodeQuery(code));
        if (result.IsFailure) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create promotion (Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var result = await _mediator.Send(new CreatePromotionCommand(
            request.Name, request.Description, request.DiscountType, request.DiscountValue,
            request.MinOrderAmount, request.MaxDiscountAmount, request.StartDate, request.EndDate, request.UsageLimit));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetPromotion), new { code = result.Value!.Code }, result.Value);
    }

    /// <summary>
    /// Update promotion (Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPut("{code}")]
    public async Task<IActionResult> UpdatePromotion(string code, [FromBody] UpdatePromotionRequest request)
    {
        var result = await _mediator.Send(new UpdatePromotionCommand(
            code, request.Name, request.Description, request.DiscountValue,
            request.MinOrderAmount, request.MaxDiscountAmount, request.StartDate, request.EndDate, 
            request.UsageLimit, request.IsActive));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete promotion (Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeletePromotion(string code)
    {
        var result = await _mediator.Send(new DeletePromotionCommand(code));
        if (result.IsFailure) return BadRequest(result.Error);
        return NoContent();
    }
}

public record CreatePromotionRequest(
    string Name, string? Description, string DiscountType, decimal DiscountValue,
    decimal? MinOrderAmount, decimal? MaxDiscountAmount, DateTime StartDate, DateTime EndDate, int? UsageLimit);

public record UpdatePromotionRequest(
    string? Name, string? Description, decimal? DiscountValue,
    decimal? MinOrderAmount, decimal? MaxDiscountAmount, DateTime? StartDate, DateTime? EndDate, 
    int? UsageLimit, bool? IsActive);
