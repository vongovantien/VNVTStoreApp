using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.Delivery.Commands;
using VNVTStore.Application.Delivery.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.API.Controllers.v1;

public class DeliveryController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public DeliveryController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    private string GetUserCode()
    {
        return _currentUser.UserCode ?? throw new UnauthorizedAccessException();
    }

    [HttpPost("order/{orderCode}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AssignDelivery(string orderCode, [FromBody] AssignDeliveryDto dto)
    {
        var result = await Mediator.Send(new AssignDeliveryCommand(GetUserCode(), orderCode, dto));
        return HandleCreated(result, nameof(GetDeliveryByOrder), new { orderCode }, "Delivery assigned successfully");
    }

    [HttpPut("{deliveryCode}/status")]
    [Authorize(Roles = "Admin,Staff,Shipper")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDeliveryStatus(string deliveryCode, [FromBody] UpdateDeliveryStatusDto dto)
    {
        var result = await Mediator.Send(new UpdateDeliveryStatusCommand(GetUserCode(), deliveryCode, dto.Status, dto.Note, dto.Location));
        return HandleResult(result, "Delivery status updated successfully");
    }

    [HttpGet("order/{orderCode}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveryByOrder(string orderCode)
    {
        var result = await Mediator.Send(new GetDeliveryByOrderQuery(orderCode));
        return HandleResult(result, MessageConstants.Get(MessageConstants.Success));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Shipper")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeliveryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveries(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? shipperCode,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var effectiveShipperCode = shipperCode;
        if (string.Equals(_currentUser.Role, "Shipper", StringComparison.OrdinalIgnoreCase) && !_currentUser.IsAdmin)
        {
            effectiveShipperCode = GetUserCode();
        }
        var result = await Mediator.Send(new GetDeliveriesQuery(status, search, effectiveShipperCode, pageIndex, pageSize));
        return HandleResult(result);
    }

    [HttpGet("my-tasks")]
    [Authorize(Roles = "Shipper,Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeliveryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks([FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
        var result = await Mediator.Send(new GetDeliveriesQuery(status, null, GetUserCode(), pageIndex, pageSize));
        return HandleResult(result);
    }

    [HttpGet("{deliveryCode}/history")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<DeliveryHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveryHistory(string deliveryCode)
    {
        var result = await Mediator.Send(new GetDeliveryHistoryQuery(deliveryCode));
        return HandleResult(result, MessageConstants.Get(MessageConstants.Success));
    }
}
