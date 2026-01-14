using AutoMapper;
using MediatR;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IRepository<TblUser> repository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if username exists
        var existingUser = await _repository.FindAsync(u => u.Username == request.username, cancellationToken);
        if (existingUser != null)
            return Result.Failure<UserDto>(Error.Conflict(MessageConstants.UsernameExists));

        // Check if email exists
        existingUser = await _repository.FindAsync(u => u.Email == request.email, cancellationToken);
        if (existingUser != null)
            return Result.Failure<UserDto>(Error.Conflict(MessageConstants.EmailInUse));

        // Use Rich Domain Model Factory
        var user = TblUser.Create(
            request.username, 
            request.email, 
            _passwordHasher.Hash(request.password), 
            request.fullName, 
            request.email.Contains("admin") ? UserRole.Admin : UserRole.Customer
        );

        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IRepository<TblUser> repository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.FindAsync(u => u.Username == request.username || u.Email == request.username, cancellationToken);
        
        if (user == null || !_passwordHasher.Verify(request.password, user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>(Error.Validation(MessageConstants.InvalidCredentials));
        }
        
        var token = _jwtService.GenerateToken(user.Code, user.Username, user.Email, user.Role);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7)); // 7 days expiry
        user.UpdateLastLogin();
        
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        });
    }
}
