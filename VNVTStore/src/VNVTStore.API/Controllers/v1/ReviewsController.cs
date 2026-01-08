using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Reviews.Commands;
using VNVTStore.Application.Reviews.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public ReviewsController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get reviews for a product
    /// </summary>
    [HttpGet("product/{productCode}")]
    public async Task<IActionResult> GetProductReviews(string productCode, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetProductReviewsQuery(productCode, pageIndex, pageSize));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get current user's reviews
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await _mediator.Send(new GetUserReviewsQuery(userCode));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a review for a purchased item
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await _mediator.Send(new CreateReviewCommand(
            userCode, request.OrderItemCode, request.Rating, request.Title, request.Content));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetProductReviews), new { productCode = "unknown" }, result.Value);
    }

    /// <summary>
    /// Update a review
    /// </summary>
    [Authorize]
    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateReview(string code, [FromBody] UpdateReviewRequest request)
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await _mediator.Send(new UpdateReviewCommand(
            code, userCode, request.Rating, request.Title, request.Content));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    [Authorize]
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteReview(string code)
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await _mediator.Send(new DeleteReviewCommand(code, userCode));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return NoContent();
    }
}

public record CreateReviewRequest(string OrderItemCode, int Rating, string? Title, string? Content);
public record UpdateReviewRequest(int? Rating, string? Title, string? Content);
