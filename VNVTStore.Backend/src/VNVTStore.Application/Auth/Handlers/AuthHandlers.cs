using AutoMapper;
using MediatR;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace VNVTStore.Application.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IRepository<TblUser> repository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
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

        // Send Verification Email
        var verificationLink = $"http://localhost:5173/verify-email?email={user.Email}&token={user.EmailVerificationToken}";
        await _emailService.SendEmailAsync(user.Email, "VNVT Store - Email Verification", 
            $"<h1>Welcome to VNVT Store!</h1><p>Please verify your email by clicking the following link:</p><a href='{verificationLink}'>Verify Email</a>", true);

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<bool>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IRepository<TblUser> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.FindAsync(u => u.Email == request.email, cancellationToken);
        if (user == null)
            return Result.Failure<bool>(Error.NotFound("User not found"));

        try
        {
            user.VerifyEmail(request.token);
            _repository.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Validation(ex.Message));
        }
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IRepository<TblUser> repository, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.FindAsync(u => u.Email == request.email, cancellationToken);
        if (user == null) return Result.Success(true); // Don't reveal account existence

        user.GeneratePasswordResetToken();
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        var resetLink = $"http://localhost:5173/reset-password?email={user.Email}&token={user.PasswordResetToken}";
        await _emailService.SendEmailAsync(user.Email, "VNVT Store - Reset Password",
            $"<h1>Reset Your Password</h1><p>Click the link below to reset your password:</p><a href='{resetLink}'>Reset Password</a>", true);

        return Result.Success(true);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(IRepository<TblUser> repository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.FindAsync(u => u.Email == request.email, cancellationToken);
        if (user == null) return Result.Failure<bool>(Error.NotFound("User not found"));

        try
        {
            user.ResetPassword(request.token, _passwordHasher.Hash(request.newPassword));
            _repository.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Validation(ex.Message));
        }
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
        // Use Where + Include to get Role, Permissions, and Menus
        var user = await _repository.Where(u => u.Username == request.username || u.Email == request.username)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRolePermissions)
                    .ThenInclude(rp => rp.PermissionCodeNavigation)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRoleMenus)
                    .ThenInclude(rm => rm.MenuCodeNavigation)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (user == null || !_passwordHasher.Verify(request.password, user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>(Error.Validation(MessageConstants.InvalidCredentials));
        }
        
        var permissions = user.RoleCodeNavigation?.TblRolePermissions
            .Where(rp => rp.PermissionCodeNavigation != null)
            .Select(rp => rp.PermissionCodeNavigation!.Name)
            .ToList() ?? new List<string>();
            
        var menus = user.RoleCodeNavigation?.TblRoleMenus
            .Where(rm => rm.MenuCodeNavigation != null)
            .Select(rm => rm.MenuCodeNavigation!.Code)
            .ToList() ?? new List<string>();

        var token = _jwtService.GenerateToken(user.Code, user.Username, user.Email, user.Role, permissions, menus);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7)); // 7 days expiry
        user.UpdateLastLogin();
        
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        };
        
        responseDto.User.Permissions = permissions;
        responseDto.User.Menus = menus;

        return Result.Success(responseDto);
    }
}
