using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Dashboard.Queries;

namespace VNVTStore.API.Controllers.v1;

[Authorize(Roles = "admin")]
public class DashboardController : BaseApiController
{
    public DashboardController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get dashboard statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetDashboardStatsQuery());
        return HandleResult(result);
    }
}
