using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Auth.Commands;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string? FullName
) : IRequest<Result<UserDto>>;

public record LoginCommand(
    string Username,
    string Password
) : IRequest<Result<AuthResponseDto>>;
