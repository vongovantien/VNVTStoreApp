using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Services;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Auth.Handlers;

public class SetupTwoFactorCommandHandler : IRequestHandler<SetupTwoFactorCommand, Result<TwoFactorSetupDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly ITotpService _totpService;

    public SetupTwoFactorCommandHandler(IRepository<TblUser> repository, ITotpService totpService)
    {
        _repository = repository;
        _totpService = totpService;
    }

    public async Task<Result<TwoFactorSetupDto>> Handle(SetupTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.UserCode, cancellationToken);
        if (user == null)
        {
            return Result.Failure<TwoFactorSetupDto>(Error.NotFound("User", request.UserCode));
        }

        // Generate a new secret key
        var secretKey = _totpService.GenerateSecretKey();
        var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email, secretKey, "VNVTStore");

        return Result.Success(new TwoFactorSetupDto
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            ManualEntryKey = secretKey
        });
    }
}

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result<TwoFactorEnableDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;

    public EnableTwoFactorCommandHandler(
        IRepository<TblUser> repository,
        ITotpService totpService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TwoFactorEnableDto>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.UserCode, cancellationToken);
        if (user == null)
        {
            return Result.Failure<TwoFactorEnableDto>(Error.NotFound("User", request.UserCode));
        }

        // Extract secret and code from request (code format could be "SECRET:CODE" or separate)
        // Request.Code is expected to be "secretKey|6DigitCode"
        var parts = request.Code.Split('|', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return Result.Failure<TwoFactorEnableDto>(Error.Validation("Dữ liệu xác thực không hợp lệ. Vui lòng cung cấp secret key và mã xác thực."));
        }

        var secretKey = parts[0];
        var totpCode = parts[1];

        if (!_totpService.VerifyTotp(secretKey, totpCode))
        {
            return Result.Failure<TwoFactorEnableDto>(Error.Validation("Mã xác thực 6 chữ số không chính xác hoặc đã hết hạn."));
        }

        var recoveryCodes = _totpService.GenerateRecoveryCodes(8);
        var recoveryCodesString = string.Join(",", recoveryCodes);

        user.EnableTwoFactor(secretKey, recoveryCodesString);
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new TwoFactorEnableDto
        {
            RecoveryCodes = recoveryCodes
        });
    }
}

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Result<bool>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;

    public DisableTwoFactorCommandHandler(
        IRepository<TblUser> repository,
        IPasswordHasher passwordHasher,
        ITotpService totpService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByCodeAsync(request.UserCode, cancellationToken);
        if (user == null)
        {
            return Result.Failure<bool>(Error.NotFound("User", request.UserCode));
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<bool>(Error.Validation("Mật khẩu hiện tại không chính xác."));
        }

        // Verify either TOTP code or recovery code
        bool isCodeValid = false;
        if (!string.IsNullOrWhiteSpace(user.TwoFactorSecret) && _totpService.VerifyTotp(user.TwoFactorSecret, request.Code))
        {
            isCodeValid = true;
        }
        else if (user.ConsumeRecoveryCode(request.Code))
        {
            isCodeValid = true;
        }

        if (!isCodeValid)
        {
            return Result.Failure<bool>(Error.Validation("Mã xác thực hoặc mã khôi phục không chính xác."));
        }

        user.DisableTwoFactor();
        _repository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}

public class VerifyTwoFactorLoginCommandHandler : IRequestHandler<VerifyTwoFactorLoginCommand, Result<AuthResponseDto>>
{
    private readonly IRepository<TblUser> _repository;
    private readonly IJwtService _jwtService;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VerifyTwoFactorLoginCommandHandler(
        IRepository<TblUser> repository,
        IJwtService jwtService,
        ITotpService totpService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _jwtService = jwtService;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(VerifyTwoFactorLoginCommand request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = _jwtService.GetPrincipalFromExpiredToken(request.TwoFactorToken);
        }
        catch
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Phiên xác thực 2FA đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại."));
        }

        var userCode = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userCode))
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Mã định danh phiên xác thực không hợp lệ."));
        }

        var user = await _repository.Where(u => u.Code == userCode)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRolePermissions)
                    .ThenInclude(rp => rp.PermissionCodeNavigation)
            .Include(u => u.RoleCodeNavigation)
                .ThenInclude(r => r.TblRoleMenus)
                    .ThenInclude(rm => rm.MenuCodeNavigation)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result.Failure<AuthResponseDto>(Error.NotFound("User", userCode));
        }

        if (user.IsLockedOut)
        {
            var remainingMinutes = Math.Ceiling((user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes);
            return Result.Failure<AuthResponseDto>(Error.Validation($"Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau {Math.Max(1, (int)remainingMinutes)} phút."));
        }

        // Verify either TOTP code or recovery code
        bool isCodeValid = false;
        if (!string.IsNullOrWhiteSpace(user.TwoFactorSecret) && _totpService.VerifyTotp(user.TwoFactorSecret, request.Code))
        {
            isCodeValid = true;
        }
        else if (user.ConsumeRecoveryCode(request.Code))
        {
            isCodeValid = true;
        }

        if (!isCodeValid)
        {
            return Result.Failure<AuthResponseDto>(Error.Validation("Mã xác thực hoặc mã khôi phục không chính xác."));
        }

        // Reset lockout count on successful 2FA
        if (user.AccessFailedCount > 0)
        {
            user.ResetAccessFailedCount();
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

        var token = _jwtService.GenerateToken(user.Code, user.Username, user.Email, user.RoleCode ?? "CUSTOMER", permissions, menus);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
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
