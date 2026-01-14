using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Users.Queries;

public record GetUserProfileQuery(string userCode) : IRequest<Result<UserDto>>;

public record GetAllUsersQuery(
    int pageIndex = 1,
    int pageSize = 10,
    string? search = null,
    UserRole? role = null,
    SortDTO? sort = null,
    List<SearchDTO>? filters = null
) : IRequest<Result<PagedResult<UserDto>>>;
