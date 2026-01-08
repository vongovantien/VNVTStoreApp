using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FullName);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));
        }

        return Ok(ApiResponse<UserDto>.Ok(result.Value!, "User registered successfully"));
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(ApiResponse<string>.Fail(result.Error!.Message, 401));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!, "Login successful"));
    }
}

// Request DTOs
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? FullName = null
);

public record LoginRequest(
    string Username,
    string Password
);
