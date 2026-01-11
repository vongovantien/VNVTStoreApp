using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Users.Queries;

public record GetUserProfileQuery(string UserCode) : IRequest<Result<UserDto>>;

public record GetAllUsersQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    string? Role = null,
    SortDTO? Sort = null,
    List<SearchDTO>? Filters = null
) : IRequest<Result<PagedResult<UserDto>>>;
