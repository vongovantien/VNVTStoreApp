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
