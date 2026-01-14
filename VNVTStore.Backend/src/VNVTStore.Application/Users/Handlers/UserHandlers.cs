using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Users.Handlers;

public class UserHandlers :
    IRequestHandler<GetUserProfileQuery, Result<UserDto>>,
    IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>,
    IRequestHandler<UpdateProfileCommand, Result<UserDto>>,
    IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IRepository<TblUser> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserHandlers(
        IRepository<TblUser> userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByCodeAsync(request.userCode, cancellationToken);
        
        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.userCode));

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.AsQueryable();

        if (!string.IsNullOrEmpty(request.search))
        {
            query = query.Where(u => 
                u.Username.Contains(request.search) ||
                u.Email.Contains(request.search) ||
                (u.FullName != null && u.FullName.Contains(request.search)));
        }

        if (request.role.HasValue)
        {
            query = query.Where(u => u.Role == request.role.Value);
        }

        // Apply advanced filters
        query = QueryHelper.ApplyFilters(query, request.filters);

        var totalItems = await query.CountAsync(cancellationToken);

        // Sort
        if (request.sort != null && !string.IsNullOrEmpty(request.sort.SortBy))
        {
            // Simple manual sort for common fields to avoid reflection/dynamic linq complexity
            // Ideally should use a helper or Dynamic Linq
            switch (request.sort.SortBy.ToLower())
            {
                case "fullname":
                    query = request.sort.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName);
                    break;
                case "email":
                    query = request.sort.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                    break;
                case "createdat":
                    query = request.sort.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                    break;
                 case "role":
                    query = request.sort.SortDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role);
                    break;
                default:
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var items = await query
            .Skip((request.pageIndex - 1) * request.pageSize)
            .Take(request.pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<UserDto>>(items);
        return Result.Success(new PagedResult<UserDto>(dtos, totalItems));
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByCodeAsync(request.userCode, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.userCode));

        // Update fields if provided
        // Use Domain Method for validation and encapsulation
        user.UpdateProfile(
            request.fullName, 
            request.phone, 
            request.email);
        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByCodeAsync(request.userCode, cancellationToken);

        if (user == null)
            return Result.Failure<bool>(Error.NotFound(MessageConstants.User, request.userCode));

        // Verify current password
        if (!_passwordHasher.Verify(request.currentPassword, user.PasswordHash))
            return Result.Failure<bool>(Error.Validation(MessageConstants.CurrentPasswordIncorrect));

        // Update password using Domain Method
        user.UpdatePassword(_passwordHasher.Hash(request.newPassword));
        
        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
