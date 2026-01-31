using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin.Auth;
using FirebaseAdmin;

namespace VNVTStore.Application.Auth.Commands;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public ExternalLoginCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        // 0. Check Firebase Initialization
        if (FirebaseApp.DefaultInstance == null)
        {
            return Result<AuthResponseDto>.Failure("Firebase Admin SDK is not initialized. Please check backend logs.");
        }

        // 1. Verify token with Firebase
        string email;
        string name;
        string providerKey;
        string? avatar = null;

        FirebaseToken decodedToken;
        try
        {
            if (FirebaseAuth.DefaultInstance == null)
            {
                return Result<AuthResponseDto>.Failure("FirebaseAuth.DefaultInstance is null even though app is initialized.");
            }

            decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Token);
            if (decodedToken == null)
            {
                return Result<AuthResponseDto>.Failure("Firebase token verification returned null.");
            }

            if (decodedToken.Claims == null)
            {
                return Result<AuthResponseDto>.Failure("Firebase token claims are null.");
            }

            email = decodedToken.Claims.ContainsKey("email") && decodedToken.Claims["email"] != null 
                ? decodedToken.Claims["email"].ToString()! 
                : null!;
            
            if (string.IsNullOrEmpty(email))
            {
                return Result<AuthResponseDto>.Failure("Email claim is missing or empty in Firebase token.");
            }

            name = decodedToken.Claims.ContainsKey("name") && decodedToken.Claims["name"] != null 
                ? decodedToken.Claims["name"].ToString()! 
                : email.Split('@')[0];
            
            providerKey = decodedToken.Uid;
            if (string.IsNullOrEmpty(providerKey))
            {
                return Result<AuthResponseDto>.Failure("UID is missing in Firebase token.");
            }

            avatar = decodedToken.Claims.ContainsKey("picture") && decodedToken.Claims["picture"] != null 
                ? decodedToken.Claims["picture"].ToString() 
                : null;
        }
        catch (Exception ex)
        {
            var fullError = $"Invalid Firebase Token: {ex.Message} | StackTrace: {ex.StackTrace}";
            if (ex.InnerException != null)
            {
                fullError += $" | Inner: {ex.InnerException.Message}";
            }
            Console.WriteLine($"[Error] Firebase verification failed: {fullError}");
            return Result<AuthResponseDto>.Failure(fullError);
        }

        // 2. Check if this External Login exists
        var userLogin = await _context.TblUserLogins
            .Include(ul => ul.User)
            .FirstOrDefaultAsync(ul => ul.LoginProvider == request.Provider && ul.ProviderKey == providerKey, cancellationToken);

        TblUser user;

        if (userLogin != null)
        {
            user = userLogin.User;
        }
        else
        {
            user = await _context.TblUsers
                .Include(u => u.TblUserLogins)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
            {
                user = TblUser.CreateExternal(email, name, request.Provider, providerKey, email, avatar);
                _context.TblUsers.Add(user);
            }
            else
            {
                user.AddLogin(request.Provider, providerKey, email);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Account is locked.");
        }

        // 3. Generate Tokens using IJwtService
        var accessToken = _jwtService.GenerateToken(user.Code, user.Username, user.Email, user.Role);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        user.UpdateLastLogin();
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Code = user.Code,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                Avatar = user.AvatarUrl
            }
        });
    }
}
