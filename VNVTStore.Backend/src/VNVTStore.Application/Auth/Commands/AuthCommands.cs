using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Auth.Commands;

public record RegisterCommand(
    string username,
    string email,
    string password,
    string? fullName
) : IRequest<Result<UserDto>>;

public record LoginCommand(
    string username,
    string password
) : IRequest<Result<AuthResponseDto>>;

public record VerifyEmailCommand(
    string email,
    string token
) : IRequest<Result<bool>>;

public record ForgotPasswordCommand(
    string email
) : IRequest<Result<bool>>;

public record ResetPasswordCommand(
    string email,
    string token,
    string newPassword
) : IRequest<Result<bool>>;

public record ImpersonateCommand(
    string targetUserCode
) : IRequest<Result<AuthResponseDto>>;
