using System;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using Xunit;

namespace VNVTStore.Tests.Domain;

public class TblUserTests
{
    [Fact]
    public void Create_ShouldInitializeVerificationToken()
    {
        // Act
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test Name", UserRole.Customer);

        // Assert
        Assert.False(user.IsEmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
    }

    [Fact]
    public void VerifyEmail_WithCorrectToken_ShouldWork()
    {
        // Arrange
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test Name", UserRole.Customer);
        var token = user.EmailVerificationToken!;

        // Act
        user.VerifyEmail(token);

        // Assert
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationToken);
    }

    [Fact]
    public void VerifyEmail_WithWrongToken_ShouldThrow()
    {
        // Arrange
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test Name", UserRole.Customer);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.VerifyEmail("wrong-token"));
    }

    [Fact]
    public void GeneratePasswordResetToken_ShouldSetTokenAndExpiry()
    {
        // Arrange
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test Name", UserRole.Customer);

        // Act
        user.GeneratePasswordResetToken();

        // Assert
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.ResetTokenExpiry);
        Assert.True(user.ResetTokenExpiry > DateTime.UtcNow);
    }

    [Fact]
    public void ResetPassword_WithValidToken_ShouldChangeHash()
    {
        // Arrange
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test Name", UserRole.Customer);
        user.GeneratePasswordResetToken();
        var token = user.PasswordResetToken!;
        var newHash = "new-hash";

        // Act
        user.ResetPassword(token, newHash);

        // Assert
        Assert.Equal(newHash, user.PasswordHash);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.ResetTokenExpiry);
    }
}
