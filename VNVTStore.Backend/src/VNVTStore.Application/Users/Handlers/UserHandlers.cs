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
    IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>,
    IRequestHandler<UpdateProfileCommand, Result<UserDto>>,
    IRequestHandler<ChangePasswordCommand, Result<bool>>,
    IRequestHandler<DeleteCommand<TblUser>, Result>,
    IRequestHandler<DeleteMultipleCommand<TblUser>, Result>,
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

    public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.userCode, cancellationToken);
        
        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.userCode));

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.AsQueryable();
        
        // Filter Soft Deleted
        query = query.Where(u => u.ModifiedType != ModificationType.Delete.ToString());

        if (!string.IsNullOrEmpty(request.search))
        {
            query = query.Where(u => 
                u.Username.Contains(request.search) ||
                u.Email.Contains(request.search) ||
                (u.FullName != null && u.FullName.Contains(request.search)));
        }

        // ... (rest of query building is same) ...
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
        var user = await _repository.GetByCodeAsync(request.userCode, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound(MessageConstants.User, request.userCode));

        // Update fields if provided
        // Use Domain Method for validation and encapsulation
        user.UpdateProfile(
            request.fullName, 
            request.phone, 
            request.email);
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
            user.UpdateProfile(dto.FullName, dto.Phone, dto.Email);

            // Update Role
            if (!string.IsNullOrEmpty(dto.Role) && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            {
                user.UpdateRole(role);
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
