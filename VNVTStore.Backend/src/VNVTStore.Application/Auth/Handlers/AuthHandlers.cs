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
using Microsoft.Extensions.Configuration;


namespace VNVTStore.Application.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        IRepository<TblUser> repository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _configuration = configuration;
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
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        var verificationLink = $"{frontendUrl}/verify-email?email={user.Email}&token={user.EmailVerificationToken}";
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
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(IRepository<TblUser> repository, IUnitOfWork unitOfWork, IEmailService emailService, IConfiguration configuration)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.FindAsync(u => u.Email == request.email, cancellationToken);
        if (user == null) return Result.Success(true); // Don't reveal account existence

        user.GeneratePasswordResetToken();
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?email={user.Email}&token={user.PasswordResetToken}";
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
        
        if (user == null)
        {
            Console.WriteLine($"DEBUG: LoginHandler - User '{request.username}' NOT found in database.");
            return Result.Failure<AuthResponseDto>(Error.Validation(MessageConstants.InvalidCredentials));
        }

        var isPasswordValid = _passwordHasher.Verify(request.password, user.PasswordHash);
        Console.WriteLine($"DEBUG: LoginHandler - User found: {user.Username}, Provided Password: '{request.password}', Hash in DB: '{user.PasswordHash}', IsValid: {isPasswordValid}");

        if (!isPasswordValid)
        {
            return Result.Failure<AuthResponseDto>(Error.Validation(MessageConstants.InvalidCredentials));
        }
        
        var permissions = new List<string>();
        if (user.RoleCodeNavigation?.TblRolePermissions != null)
        {
            permissions = user.RoleCodeNavigation.TblRolePermissions
                .Where(rp => rp.PermissionCodeNavigation != null)
                .Select(rp => rp.PermissionCodeNavigation!.Name)
                .ToList();
        }
            
        var menus = new List<string>();
        if (user.RoleCodeNavigation?.TblRoleMenus != null)
        {
            menus = user.RoleCodeNavigation.TblRoleMenus
                .Where(rm => rm.MenuCodeNavigation != null)
                .Select(rm => rm.MenuCodeNavigation!.Code)
                .ToList();
        }

        Console.WriteLine($"DEBUG: LoginHandler - User: {user.Username}, Role: {user.Role}, RoleCode: {user.RoleCode}");
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

public class ImpersonateCommandHandler : IRequestHandler<ImpersonateCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUserService;

    public ImpersonateCommandHandler(
        IRepository<TblUser> repository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUser currentUserService)
    {
        _repository = repository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AuthResponseDto>> Handle(ImpersonateCommand request, CancellationToken cancellationToken)
    {
        // 1. Authorization check: Only Admin can impersonate
        // Ideally, we check this via [Authorize(Roles = "Admin")] on the controller,
        // but adding an extra check here for defense-in-depth.
        var currentUserCode = _currentUserService.UserCode;
        var adminUser = await _repository.FindAsync(u => u.Code == currentUserCode, cancellationToken);
        
        if (adminUser == null || adminUser.Role != UserRole.Admin)
        {
            return Result.Failure<AuthResponseDto>(Error.Forbidden("Only administrators can impersonate users."));
        }

        // 2. Find target user (Try Code first, then Username)
        var targetUser = await _repository.Where(u => u.Code == request.targetUserCode || u.Username == request.targetUserCode)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRolePermissions)
                    .ThenInclude(rp => rp.PermissionCodeNavigation)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRoleMenus)
                    .ThenInclude(rm => rm.MenuCodeNavigation)
            .FirstOrDefaultAsync(cancellationToken);

        if (targetUser == null)
        {
            return Result.Failure<AuthResponseDto>(Error.NotFound("Target user not found."));
        }

        // 3. Prevent impersonating other admins (security policy)
        if (targetUser.Role == UserRole.Admin)
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Cannot impersonate another administrator."));
        }

        // 4. Generate token for target user
        var permissions = targetUser.RoleCodeNavigation?.TblRolePermissions
            .Where(rp => rp.PermissionCodeNavigation != null)
            .Select(rp => rp.PermissionCodeNavigation!.Name)
            .ToList() ?? new List<string>();
            
        var menus = targetUser.RoleCodeNavigation?.TblRoleMenus
            .Where(rm => rm.MenuCodeNavigation != null)
            .Select(rm => rm.MenuCodeNavigation!.Code)
            .ToList() ?? new List<string>();

        var token = _jwtService.GenerateToken(targetUser.Code, targetUser.Username, targetUser.Email, targetUser.Role, permissions, menus);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Note: We don't necessarily want to update target user's LastLogin during impersonation 
        // to avoid skewing analytics, but we MUST set a valid refresh token.
        targetUser.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(1)); // Shorter expiry for impersonation tokens
        
        _repository.Update(targetUser);
        await _unitOfWork.CommitAsync(cancellationToken);

        var responseDto = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(targetUser)
        };
        
        responseDto.User.Permissions = permissions;
        responseDto.User.Menus = menus;

        return Result.Success(responseDto);
    }
}
