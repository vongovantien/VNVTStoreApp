using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Users.Handlers;

public class UserHandlers : BaseHandler<TblUser>,
    IRequestHandler<GetUserProfileQuery, Result<UserDto>>,
    IRequestHandler<GetPagedQuery<UserDto>, Result<PagedResult<UserDto>>>,
    IRequestHandler<UpdateProfileCommand, Result<UserDto>>,
    IRequestHandler<ChangePasswordCommand, Result<bool>>,
    IRequestHandler<DeleteCommand<TblUser>, Result>,
    IRequestHandler<DeleteMultipleCommand<TblUser>, Result>,
    IRequestHandler<GetByCodeQuery<UserDto>, Result<UserDto>>,
    IRequestHandler<CreateCommand<CreateUserDto, UserDto>, Result<UserDto>>,
    IRequestHandler<UpdateCommand<UpdateUserDto, UserDto>, Result<UserDto>>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<TblOrder> _orderRepository;

    public UserHandlers(
        IRepository<TblUser> userRepository,
        IRepository<TblOrder> orderRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(userRepository, unitOfWork, mapper, dapperContext)

    {
        _passwordHasher = passwordHasher;
        _orderRepository = orderRepository;
    }

    public async Task<Result<UserDto>> Handle(GetByCodeQuery<UserDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<UserDto>(request.Code, MessageConstants.User, cancellationToken);
    }

    public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<UserDto>(request.userCode, MessageConstants.User, cancellationToken);
    }


    public async Task<Result<PagedResult<UserDto>>> Handle(GetPagedQuery<UserDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedDapperAsync<UserDto>(
            request.PageIndex, 
            request.PageSize, 
            request.Searching, 
            request.SortDTO, 
            null, // referenceTables
            request.Fields, 
            cancellationToken);
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.userCode, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.userCode));

        // Update fields if provided
        // Use Domain Method for validation and encapsulation
        user.UpdateProfile(
            request.fullName, 
            request.phone, 
            request.email,
            request.avatarUrl);
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.userCode, cancellationToken);

        if (user == null)
            return Result.Failure<bool>(Error.NotFound(MessageConstants.User, request.userCode));

        // Verify current password
        if (!_passwordHasher.Verify(request.currentPassword, user.PasswordHash))
            return Result.Failure<bool>(Error.Validation(MessageConstants.CurrentPasswordIncorrect));

        // Update password using Domain Method
        user.UpdatePassword(_passwordHasher.Hash(request.newPassword));
        
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
    
    // ...

    public async Task<Result<UserDto>> Handle(CreateCommand<CreateUserDto, UserDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;

            // Check if username or email exists
            var existingUser = await _repository.FindAsync(u => u.Username == dto.Username || u.Email == dto.Email, cancellationToken);
            if (existingUser != null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<UserDto>(Error.Conflict(MessageConstants.AlreadyExists, "Username/Email", dto.Username));
            }

            // Parse Role
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            {
                role = UserRole.Customer; // Default
            }

            var passwordHash = _passwordHasher.Hash(dto.Password);

            var user = TblUser.Create(dto.Username, dto.Email, passwordHash, dto.FullName, role);
            user.IsActive = dto.IsActive;
            if (dto.Phone != null) user.UpdateProfile(dto.FullName, dto.Phone, dto.Email);

            await _repository.AddAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<UserDto>(user));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<UserDto>> Handle(UpdateCommand<UpdateUserDto, UserDto> request, CancellationToken cancellationToken)
    {
         await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (user == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.Code));
            }

            var dto = request.Dto;

            // Update Profile
            user.UpdateProfile(dto.FullName, dto.Phone, dto.Email, dto.AvatarUrl);

            // Update Role
            if (!string.IsNullOrEmpty(dto.Role) && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            {
                user.UpdateRoleEnum(role);
            }

            // Update Active Status
            if (dto.IsActive.HasValue)
            {
                user.IsActive = dto.IsActive.Value;
            }

            // Update Password if provided (Admin reset)
            if (!string.IsNullOrEmpty(dto.Password))
            {
                 user.UpdatePassword(_passwordHasher.Hash(dto.Password));
            }

            _repository.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<UserDto>(user));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
    public async Task<Result> Handle(DeleteCommand<TblUser> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.User, cancellationToken);
    }

    public async Task<Result> Handle(DeleteMultipleCommand<TblUser> request, CancellationToken cancellationToken)
    {
        return await DeleteMultipleAsync(request.Codes, MessageConstants.User, cancellationToken);
    }
}
