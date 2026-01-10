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
        var user = await _userRepository.GetByCodeAsync(request.UserCode, cancellationToken);
        
        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.UserCode));

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(u => 
                u.Username.Contains(request.Search) ||
                u.Email.Contains(request.Search) ||
                (u.FullName != null && u.FullName.Contains(request.Search)));
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(u => u.Role == request.Role);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<UserDto>>(items);
        return Result.Success(new PagedResult<UserDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByCodeAsync(request.UserCode, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.UserCode));

        // Update fields if provided
        // Use Domain Method for validation and encapsulation
        user.UpdateProfile(
            request.FullName ?? user.FullName, 
            request.Phone ?? user.Phone, 
            request.Email ?? user.Email);
        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByCodeAsync(request.UserCode, cancellationToken);

        if (user == null)
            return Result.Failure<bool>(Error.NotFound(MessageConstants.User, request.UserCode));

        // Verify current password
        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure<bool>(Error.Validation(MessageConstants.CurrentPasswordIncorrect));

        // Update password using Domain Method
        user.UpdatePassword(_passwordHasher.Hash(request.NewPassword));
        
        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
