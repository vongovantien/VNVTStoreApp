using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.API.Controllers.v1;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VNVTStore.Tests.Integration;

public class AuthApiTests : ApiTestBase
{
    public AuthApiTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldCreateUnverifiedUserAndReturnSuccess()
    {
        // Arrange
        var uniqueSub = Guid.NewGuid().ToString("N").Substring(0, 5);
        var request = new RegisterRequest($"user_{uniqueSub}", $"test_{uniqueSub}@example.com", "Password123!", "Test User");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(result.Data.IsEmailVerified);
        
        // Verify in DB
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
        var user = await repo.FindAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.False(user.IsEmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldActivateUser()
    {
        // Arrange
        var uniqueSub = Guid.NewGuid().ToString("N").Substring(0, 5);
        var email = $"verify_{uniqueSub}@example.com";
        var registerRequest = new RegisterRequest($"v_{uniqueSub}", email, "Password123!", "Verify User");
        
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Get token from DB
        string token;
        using (var scope = Factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
            var user = await repo.FindAsync(u => u.Email == email);
            token = user.EmailVerificationToken!;
        }

        // Act
        var response = await Client.GetAsync($"/api/v1/auth/verify-email?email={email}&token={token}");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.Data);

        // Verify in DB
        using (var scope = Factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
            var user = await repo.FindAsync(u => u.Email == email);
            Assert.True(user.IsEmailVerified);
            Assert.Null(user.EmailVerificationToken);
        }
    }

    [Fact]
    public async Task ForgotPassword_ShouldGenerateTokenAndReturnSuccess()
    {
        // Arrange
        var uniqueSub = Guid.NewGuid().ToString("N").Substring(0, 5);
        var email = $"forgot_{uniqueSub}@example.com";
        var registerRequest = new RegisterRequest($"f_{uniqueSub}", email, "Password123!", "Forgot User");
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify in DB
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
        var user = await repo.FindAsync(u => u.Email == email);
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.ResetTokenExpiry);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldChangePassword()
    {
        // Arrange
        var uniqueSub = Guid.NewGuid().ToString("N").Substring(0, 5);
        var email = $"reset_{uniqueSub}@example.com";
        var registerRequest = new RegisterRequest($"r_{uniqueSub}", email, "OldPass123!", "Reset User");
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        await Client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));

        string token;
        using (var scope = Factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
            var user = await repo.FindAsync(u => u.Email == email);
            token = user.PasswordResetToken!;
        }

        // Act
        var resetRequest = new ResetPasswordRequest(email, token, "NewPass456!");
        var response = await Client.PostAsJsonAsync("/api/v1/auth/reset-password", resetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify Login with new password
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest($"r_{uniqueSub}", "NewPass456!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }
}
