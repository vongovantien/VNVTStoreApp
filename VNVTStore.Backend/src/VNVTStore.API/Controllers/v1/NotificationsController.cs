using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Notifications.Commands;
using VNVTStore.Application.Notifications.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyNotifications()
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery());
        return Ok(ApiResponse<List<NotificationDto>>.Ok(result.Value ?? new List<NotificationDto>()));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadCountQuery());
        return Ok(ApiResponse<int>.Ok(result.Value));
    }

    [HttpPut("{code}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(string code)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(code));
        return Ok(ApiResponse<bool>.Ok(result.Value));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand());
        return Ok(ApiResponse<bool>.Ok(result.Value));
    }

    [HttpDelete("{code}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(string code)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(code));
        return Ok(ApiResponse<bool>.Ok(result.Value));
    }
}
