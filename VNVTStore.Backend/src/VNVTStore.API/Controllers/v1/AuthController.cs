using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.API.Controllers.v1;

[EnableRateLimiting("AuthLimit")]
public class AuthController : BaseApiController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FullName);

        var result = await Mediator.Send(command);
        return HandleResult(result, MessageConstants.Get(MessageConstants.RegisterSuccess));
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var result = await Mediator.Send(command);
        return HandleResult(result, MessageConstants.Get(MessageConstants.LoginSuccess));
    }

    /// <summary>
    /// Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Clients should send both converting/expired access token and refresh token
        var command = new RefreshTokenCommand(request.Token, request.RefreshToken);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xác thực email
    /// </summary>
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var command = new VerifyEmailCommand(email, token);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Email verified successfully");
    }

    /// <summary>
    /// Quên mật khẩu
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await Mediator.Send(command);
        return HandleResult(result, "If an account exists with this email, a reset link has been sent.");
    }

    /// <summary>
    /// Đặt lại mật khẩu
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Password reset successfully.");
    }
    /// <summary>
    /// Đăng nhập bằng mạng xã hội (Google, Firebase)
    /// </summary>
    /// <remarks>
    /// Client phải gửi Firebase ID Token (đã được Firebase SDK cấp sau khi user xác thực).
    /// Backend sẽ verify token này với Firebase Admin SDK. Không chấp nhận token chưa verify.
    /// </remarks>
    [HttpPost("external-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new ApiResponse<string> { Success = false, Message = "ID Token is required." });

        // The ExternalLoginCommand handler is responsible for verifying the ID token
        // against Firebase Admin SDK (FirebaseAuth.DefaultInstance.VerifyIdTokenAsync).
        // It must NOT trust any email or user info from the client payload directly.
        var command = new ExternalLoginCommand(request.Provider, request.Token);
        var result = await Mediator.Send(command);
        return HandleResult(result, MessageConstants.Get(MessageConstants.LoginSuccess));
    }

    /// <summary>
    /// Đăng nhập dưới quyền người dùng khác (Chỉ dành cho Admin)
    /// </summary>
    /// <param name="userCode">Mã người dùng muốn đăng nhập</param>
    [HttpPost("impersonate/{userCode}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Impersonate(string userCode)
    {
        var command = new ImpersonateCommand(userCode);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Impersonation successful");
    }
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record RefreshTokenRequest(string Token, string RefreshToken);

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
