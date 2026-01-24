using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Queries;

using VNVTStore.Application.Users.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

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
    /// Create new user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await Mediator.Send(new CreateCommand<CreateUserDto, UserDto>(dto));
        return HandleResult(result);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [Authorize(Roles = "admin,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageIndex = AppConstants.Paging.DefaultPageNumber,
        [FromQuery] int pageSize = AppConstants.Paging.DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        UserRole? userRole = null;
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            userRole = parsedRole;
        }

        var result = await Mediator.Send(new GetAllUsersQuery(pageIndex, pageSize, search, userRole));
        return HandleResult(result);
    }
    /// <summary>
    /// Search users (Admin only)
    /// </summary>
    [Authorize(Roles = "admin,Admin")]
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;
        
        var result = await Mediator.Send(new GetAllUsersQuery(pageIndex, pageSize, null, null, request.SortDTO, request.Searching));
        return HandleResult(result);
    }

    /// <summary>
    /// Get user by code (Admin only)
    /// </summary>
    [HttpGet("{code}")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await Mediator.Send(new GetUserByCodeQuery(code));
        return HandleResult(result);
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> Delete(string code)
    {
        var result = await Mediator.Send(new DeleteCommand<TblUser>(code));
        return HandleDelete(result);
    }

    /// <summary>
    /// Delete multiple users (Admin only)
    /// </summary>
    [HttpPost("delete-multiple")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
    {
        var result = await Mediator.Send(new DeleteMultipleCommand<TblUser>(codes));
        return HandleDelete(result);
    }
}

public record UpdateProfileRequest(string? fullName, string? phone, string? email);
public record ChangePasswordRequest(string currentPassword, string newPassword);
