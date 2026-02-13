using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Users.Commands;

public record UpdateProfileCommand(
    string userCode,
    string? fullName,
    string? phone,
    string? email,
    string? avatarUrl = null
) : IRequest<Result<UserDto>>;

public record ChangePasswordCommand(
    string userCode,
    string currentPassword,
    string newPassword
) : IRequest<Result<bool>>;

public record DeleteAccountCommand(
    string userCode
) : IRequest<Result<bool>>;
