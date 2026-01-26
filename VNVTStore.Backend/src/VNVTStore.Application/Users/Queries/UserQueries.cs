using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Users.Queries;

public record GetUserProfileQuery(string userCode) : IRequest<Result<UserDto>>;






