using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public OrdersController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private string GetUserCode()
    {
        return _currentUser.UserCode ?? throw new UnauthorizedAccessException();
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var command = new CreateOrderCommand(GetUserCode(), dto);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return CreatedAtAction(
            nameof(GetOrder),
            new { code = result.Value!.Code },
            ApiResponse<OrderDto>.Ok(result.Value!, "Order created successfully"));
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        var query = new GetMyOrdersQuery(GetUserCode(), pageIndex, pageSize, status);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result.Value!, "Orders retrieved successfully"));
    }

    [HttpGet("{code}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder(string code)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(code));
        
        if (result.IsFailure)
            return NotFound(ApiResponse<string>.Fail(result.Error!.Message));
            
        return Ok(ApiResponse<OrderDto>.Ok(result.Value!, "Order retrieved successfully"));
    }

    [HttpPost("{code}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelOrder(string code, [FromBody] string reason)
    {
        var command = new CancelOrderCommand(GetUserCode(), code, reason);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
             return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return Ok(ApiResponse<bool>.Ok(true, "Order cancelled successfully"));
    }

    // Admin endpoints (Update Status, Get All) can be added here
}
