using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.API.Controllers.v1;

public class CartsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public CartsController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    private string GetUserCode()
    {
        return _currentUser.UserCode ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
    {
        var result = await Mediator.Send(new GetCartQuery(GetUserCode()));
        return HandleResult(result, MessageConstants.Get(MessageConstants.CartRetrieved));
    }

    [HttpPost("items")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
    {
        var result = await Mediator.Send(new AddToCartCommand(GetUserCode(), dto.ProductCode, dto.Quantity, dto.Size, dto.Color));
        return HandleResult(result, MessageConstants.Get(MessageConstants.CartAdded));
    }

    [HttpPut("items/{itemCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem(string itemCode, [FromBody] UpdateCartItemDto dto)
    {
        var result = await Mediator.Send(new UpdateCartItemCommand(GetUserCode(), itemCode, dto.Quantity));
        return HandleResult(result, MessageConstants.Get(MessageConstants.CartUpdated));
    }

    [HttpDelete("items/{itemCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem(string itemCode)
    {
        var result = await Mediator.Send(new RemoveFromCartCommand(GetUserCode(), itemCode));
        return HandleResult(result, MessageConstants.Get(MessageConstants.CartRemoved));
    }

    [HttpDelete]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart()
    {
        var result = await Mediator.Send(new ClearCartCommand(GetUserCode()));
        return HandleDelete(result);
    }
}
