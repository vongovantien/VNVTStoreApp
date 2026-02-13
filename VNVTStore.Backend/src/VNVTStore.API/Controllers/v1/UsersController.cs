using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

namespace VNVTStore.API.Controllers.v1;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public UsersController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lấy thông tin cá nhân của người dùng hiện tại
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Unauthorized();

        var result = await Mediator.Send(new GetUserProfileQuery(userCode!));
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (request == null) return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));

        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Unauthorized();

        var command = new UpdateProfileCommand(
            userCode!,
            request.fullName,
            request.phone,
            request.email,
            request.avatarUrl);

        var result = await Mediator.Send(command);
        return HandleResult(result, MessageConstants.Get(MessageConstants.ProfileUpdated));
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request == null) return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));
        
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Unauthorized();

        // Ensure passwords are not null
        if (string.IsNullOrEmpty(request.currentPassword) || string.IsNullOrEmpty(request.newPassword))
             return BadRequest(ApiResponse<string>.Fail(MessageConstants.Get(MessageConstants.BadRequest)));

        var command = new ChangePasswordCommand(
            userCode!,
            request.currentPassword,
            request.newPassword);

        var result = await Mediator.Send(command);
        return HandleResult(result, MessageConstants.Get(MessageConstants.PasswordChanged));
    }

    /// <summary>
    /// Xóa tài khoản (Vô hiệu hóa)
    /// </summary>
    [HttpDelete("delete-account")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAccount()
    {
        var userCode = _currentUser.UserCode;
        if (string.IsNullOrEmpty(userCode)) return Unauthorized();

        var result = await Mediator.Send(new DeleteAccountCommand(userCode!));
        return HandleResult(result, "Account deactivated successfully");
    }
}

public record UpdateProfileRequest(string? fullName, string? phone, string? email, string? avatarUrl = null);
public record ChangePasswordRequest(string currentPassword, string newPassword, string confirmNewPassword);
