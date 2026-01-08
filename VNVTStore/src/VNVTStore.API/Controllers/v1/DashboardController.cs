using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Dashboard.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery());
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }
}
