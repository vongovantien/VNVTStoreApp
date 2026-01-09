using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Queries;

namespace VNVTStore.API.Controllers.v1;

public class UsersController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public UsersController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await Mediator.Send(new GetUserProfileQuery(userCode));
        return HandleResult(result);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await Mediator.Send(new UpdateProfileCommand(
            userCode, request.FullName, request.Phone, request.Email));
        
        return HandleResult(result, MessageConstants.Get(MessageConstants.ProfileUpdated));
    }

    /// <summary>
    /// Change password
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userCode = _currentUser.UserCode ?? throw new UnauthorizedAccessException();
        var result = await Mediator.Send(new ChangePasswordCommand(
            userCode, request.CurrentPassword, request.NewPassword));
        
        return HandleResult(result, MessageConstants.Get(MessageConstants.PasswordChanged));
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var result = await Mediator.Send(new GetAllUsersQuery(pageIndex, pageSize, search, role));
        return HandleResult(result);
    }
}

public record UpdateProfileRequest(string? FullName, string? Phone, string? Email);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
