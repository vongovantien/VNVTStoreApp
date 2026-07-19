using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VNVTStore.Application.Common;
using VNVTStore.Application.Delivery.Commands;
using VNVTStore.Application.Delivery.Queries;
using VNVTStore.Application.DTOs;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DeliveriesController : BaseApiController
{
    public DeliveriesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Assign shipper to an order (Admin/Staff only)
    /// </summary>
    [HttpPost("order/{orderCode}/assign")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignShipper(string orderCode, [FromBody] AssignDeliveryDto dto)
    {
        var command = new AssignDeliveryCommand(UserCode, orderCode, dto);
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<DeliveryDto>.SuccessResponse(result.Value)) : HandleFailure(result);
    }

    /// <summary>
    /// Update delivery status
    /// </summary>
    [HttpPut("{code}/status")]
    [Authorize(Roles = "Admin,Staff,Shipper")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(string code, [FromBody] UpdateDeliveryStatusDto dto)
    {
        var command = new UpdateDeliveryStatusCommand(UserCode, code, dto.Status, dto.Note, dto.Location);
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse<DeliveryDto>.SuccessResponse(result.Value)) : HandleFailure(result);
    }

    /// <summary>
    /// Get delivery details by order code
    /// </summary>
    [HttpGet("order/{orderCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrder(string orderCode)
    {
        var query = new GetDeliveryByOrderQuery(orderCode);
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(ApiResponse<DeliveryDto>.SuccessResponse(result.Value)) : HandleFailure(result);
    }

    /// <summary>
    /// Get delivery history by delivery code
    /// </summary>
    [HttpGet("{code}/history")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<DeliveryHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(string code)
    {
        var query = new GetDeliveryHistoryQuery(code);
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(ApiResponse<List<DeliveryHistoryDto>>.SuccessResponse(result.Value)) : HandleFailure(result);
    }
}
