using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
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
            userCode, request.fullName, request.phone, request.email));
        
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
            userCode, request.currentPassword, request.newPassword));
        
        return HandleResult(result, MessageConstants.Get(MessageConstants.PasswordChanged));
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageIndex = AppConstants.Paging.DefaultPageNumber,
        [FromQuery] int pageSize = AppConstants.Paging.DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var result = await Mediator.Send(new GetAllUsersQuery(pageIndex, pageSize, search, role));
        return HandleResult(result);
    }
    /// <summary>
    /// Search users (Admin only)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;
        
        // Extract basic search (name/email/username) from the generic 'Search' field if provided, 
        // OR from the 'searching' list if it contains 'search', 'name', 'code'.
        string? search = request.Searching?.FirstOrDefault(s => 
            s.Field?.ToLower() == "name" || s.Field?.ToLower() == "search" || s.Field?.ToLower() == "code" || s.Field?.ToLower() == "username")?.Value;

        // Extract advanced filters (exclude basic search fields)
        var filters = request.Searching?.Where(s => 
            s.Field?.ToLower() != "name" && s.Field?.ToLower() != "search" && s.Field?.ToLower() != "code" && s.Field?.ToLower() != "username"
        ).ToList();

        var result = await Mediator.Send(new GetAllUsersQuery(pageIndex, pageSize, search, null, request.SortDTO, filters));
        return HandleResult(result);
    }
}

public record UpdateProfileRequest(string? fullName, string? phone, string? email);
public record ChangePasswordRequest(string currentPassword, string newPassword);
