using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Auth.Commands;

public record ExternalLoginCommand(string Provider, string Token) : IRequest<Result<AuthResponseDto>>;
