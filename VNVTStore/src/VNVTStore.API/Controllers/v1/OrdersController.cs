using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.API.Controllers.v1;

public class OrdersController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public OrdersController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
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
        var result = await Mediator.Send(new CreateOrderCommand(GetUserCode(), dto));

        return HandleCreated(
            result,
            nameof(GetOrder),
            new { code = result.Value?.Code },
            "Order created successfully");
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        var result = await Mediator.Send(new GetMyOrdersQuery(GetUserCode(), pageIndex, pageSize, status));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderRetrieved));
    }

    [HttpGet("{code}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder(string code)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(code));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderRetrieved));
    }

    [HttpPost("{code}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelOrder(string code, [FromBody] string reason)
    {
        var result = await Mediator.Send(new CancelOrderCommand(GetUserCode(), code, reason));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderCancelled));
    }
}
