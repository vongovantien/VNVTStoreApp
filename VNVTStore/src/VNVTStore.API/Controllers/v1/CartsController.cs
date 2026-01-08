using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using System.Security.Claims;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser; // We need this interface to get Current User Code safely

    public CartsController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // Helper to get UserCode
    private string GetUserCode()
    {
        // If ICurrentUser handles it:
        return _currentUser.UserCode ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCart()
    {
        var result = await _mediator.Send(new GetMyCartQuery(GetUserCode()));
        return Ok(ApiResponse<CartDto>.Ok(result.Value!, "Cart retrieved successfully"));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var command = new AddToCartCommand(GetUserCode(), dto.ProductCode, dto.Quantity);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return Ok(ApiResponse<CartDto>.Ok(result.Value!, "Item added to cart"));
    }

    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
    {
        var command = new UpdateCartItemCommand(GetUserCode(), dto.ProductCode, dto.Quantity);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return Ok(ApiResponse<CartDto>.Ok(result.Value!, "Cart updated"));
    }

    [HttpDelete("{productCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveFromCart(string productCode)
    {
        var command = new RemoveFromCartCommand(GetUserCode(), productCode);
        var result = await _mediator.Send(command);
        
        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));
            
        return Ok(ApiResponse<CartDto>.Ok(result.Value!, "Item removed from cart"));
    }

    [HttpDelete]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart()
    {
        var command = new ClearCartCommand(GetUserCode());
        await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(true, "Cart cleared"));
    }
}
