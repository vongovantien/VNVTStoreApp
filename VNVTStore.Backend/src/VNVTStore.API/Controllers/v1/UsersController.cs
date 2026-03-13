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
public class UsersController : BaseApiController<TblUser, UserDto, CreateUserDto, UpdateUserDto>
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
    /// Cập nhật profile (User tự cập nhật)
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

    /// <summary>
    /// Tìm kiếm và phân trang users (Admin only filter logic in base)
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public override async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        return await base.Search(request);
    }
    /// <summary>
    /// Tạo user mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    public override async Task<IActionResult> Create([FromBody] RequestDTO<CreateUserDto> request)
    {
        return await base.Create(request);
    }

    /// <summary>
    /// Cập nhật user (Admin only)
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public override async Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateUserDto> request)
    {
        return await base.Update(code, request);
    }

    /// <summary>
    /// Xóa user (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteCommand<TblUser>(id);
        var result = await Mediator.Send(command);
        return HandleDelete(result);
    }

    /// <summary>
    /// Xóa nhiều users (Admin only)
    /// </summary>
    [HttpDelete("bulk")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteBulk([FromBody] List<string> ids)
    {
        var command = new DeleteMultipleCommand<TblUser>(ids);
        var result = await Mediator.Send(command);
        return HandleDelete(result);
    }

    /// <summary>
    /// Lấy user theo code (Admin/Staff)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCode(string id)
    {
        var query = new GetByCodeQuery<UserDto>(id);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}

public record UpdateProfileRequest(string? fullName, string? phone, string? email, string? avatarUrl = null);
public record ChangePasswordRequest(string currentPassword, string newPassword, string confirmNewPassword);
