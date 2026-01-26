using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.API.Controllers;
using VNVTStore.Domain.Enums;

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
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        string? userCode = null;
        try 
        {
             userCode = _currentUser.UserCode;
        }
        catch {}

        var result = await Mediator.Send(new CreateOrderCommand(userCode, dto));

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return HandleCreated(
            result,
            nameof(GetOrder),
            new { code = result.Value?.Code },
            "Order created successfully");
    }

    [HttpGet("verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyOrder([FromQuery] string token)
    {
        var result = await Mediator.Send(new VerifyOrderCommand(token));
        return HandleResult(result, "Order verified successfully");
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int pageIndex = AppConstants.Paging.DefaultPageNumber, [FromQuery] int pageSize = AppConstants.Paging.DefaultPageSize, [FromQuery] string? status = null)
    {
        OrderStatus? orderStatus = null;
        if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus)) 
            orderStatus = parsedStatus;

        var result = await Mediator.Send(new GetMyOrdersQuery(GetUserCode(), pageIndex, pageSize, orderStatus));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderRetrieved));
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<OrderStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStats()
    {
        var result = await Mediator.Send(new GetOrderStatsQuery());
        return HandleResult(result);
    }

    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder(string code)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(code));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderRetrieved));
    }

    [HttpPost("{code}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CancelOrder(string code, [FromBody] string reason)
    {
        var result = await Mediator.Send(new CancelOrderCommand(GetUserCode(), code, reason));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderCancelled));
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchOrders([FromBody] RequestDTO request)
    {
        OrderStatus? status = null;
        string? search = null;
        
        // Extract filters
        var filters = new Dictionary<string, string>();
        if (request.Searching != null)
        {
            foreach (var s in request.Searching)
            {
               if (!string.IsNullOrEmpty(s.SearchField) && s.SearchValue != null)
               {
                   filters[s.SearchField.ToLower()] = s.SearchValue?.ToString();
               }
            }
        }
        
        // Generic search term
        if (filters.ContainsKey("all")) search = filters["all"];
        if (filters.ContainsKey("search")) search = filters["search"];
        if (filters.ContainsKey("status") && Enum.TryParse<OrderStatus>(filters["status"], true, out var parsedStatus)) 
            status = parsedStatus;

        // Construct Query with Dictionary for advanced filtering
        var query = new GetAllOrdersQuery(
            request.PageIndex ?? AppConstants.Paging.DefaultPageNumber,
            request.PageSize ?? AppConstants.Paging.DefaultPageSize,
            status,
            search,
            filters 
        );

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}
