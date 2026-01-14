using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Users.Commands;

public record UpdateProfileCommand(
    string userCode,
    string? fullName,
    string? phone,
    string? email
) : IRequest<Result<UserDto>>;

public record ChangePasswordCommand(
    string userCode,
    string currentPassword,
    string newPassword
) : IRequest<Result<bool>>;
