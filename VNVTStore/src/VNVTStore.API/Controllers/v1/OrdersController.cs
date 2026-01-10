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
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CancelOrder(string code, [FromBody] string reason)
    {
        var result = await Mediator.Send(new CancelOrderCommand(GetUserCode(), code, reason));
        return HandleResult(result, MessageConstants.Get(MessageConstants.OrderCancelled));
    }


    
    // ... wait, I cannot easily extend the Controller without updating the Application layer.
    // I will revert to a simple mapping for now and update the Query next.
    
    // Let's implement the mapping logic, assuming I WILL update the Query in the next step.
    
    [HttpPost("search")]
    public async Task<IActionResult> SearchOrders([FromBody] RequestDTO request)
    {
        string? status = null;
        string? search = null;
        
        // Extract filters
        var filters = new Dictionary<string, string>();
        if (request.Searching != null)
        {
            foreach (var s in request.Searching)
            {
               if (!string.IsNullOrEmpty(s.Field) && s.Value != null)
               {
                   filters[s.Field.ToLower()] = s.Value;
               }
            }
        }
        
        // Generic search term
        if (filters.ContainsKey("all")) search = filters["all"];
        if (filters.ContainsKey("search")) search = filters["search"];
        if (filters.ContainsKey("status")) status = filters["status"];

        // Construct Query with Dictionary for advanced filters
        var query = new GetAllOrdersQuery(
            request.PageIndex ?? 1,
            request.PageSize ?? 10,
            status,
            search,
            filters // Passing the whole dictionary for advanced filtering
        );

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}
