using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Users.Commands;

public record UpdateProfileCommand(
    string UserCode,
    string? FullName,
    string? Phone,
    string? Email
) : IRequest<Result<UserDto>>;

public record ChangePasswordCommand(
    string UserCode,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result<bool>>;
