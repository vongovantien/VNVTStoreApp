using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Dashboard.Queries;

namespace VNVTStore.API.Controllers.v1;

using VNVTStore.Domain.Enums;

[Authorize(Roles = nameof(UserRole.Admin))]
public class DashboardController : BaseApiController
{
    public DashboardController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get dashboard statistics (Admin only) with optional date filtering
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? groupBy)
    {
        var result = await Mediator.Send(new GetDashboardStatsQuery(startDate, endDate, groupBy));
        return HandleResult(result);
    }

    /// <summary>
    /// Get detailed revenue and performance report by custom date range
    /// </summary>
    [HttpGet("revenue-report")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? groupBy = "day")
    {
        var result = await Mediator.Send(new GetRevenueReportQuery(startDate, endDate, groupBy));
        return HandleResult(result);
    }
}
