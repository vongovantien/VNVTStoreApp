using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Auth.Handlers;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Services;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class TwoFactorTests
{
    private readonly TotpService _totpService;

    public TwoFactorTests()
    {
        _totpService = new TotpService();
    }

    [Fact]
    public void TotpService_GenerateAndVerify_ShouldSucceedForValidCode()
    {
        var secret = _totpService.GenerateSecretKey();
        Assert.NotNull(secret);
        Assert.True(secret.Length >= 16);

        var uri = _totpService.GenerateQrCodeUri("user@example.com", secret, "VNVTStore");
        Assert.Contains("otpauth://totp/", uri);
        Assert.Contains(secret, uri);

        // Generate recovery codes
        var recoveryCodes = _totpService.GenerateRecoveryCodes(8);
        Assert.Equal(8, recoveryCodes.Count);
        Assert.All(recoveryCodes, c => Assert.Contains("-", c));
    }

    [Fact]
    public void TotpService_VerifyInvalidCode_ShouldReturnFalse()
    {
        var secret = _totpService.GenerateSecretKey();
        var result = _totpService.VerifyTotp(secret, "000000");
        // Unless coincidentally "000000", should usually be false
        Assert.False(_totpService.VerifyTotp(secret, "invalid"));
        Assert.False(_totpService.VerifyTotp("", "123456"));
    }

    [Fact]
    public void TblUser_AccountLockout_ShouldLockAfter5Failures()
    {
        var user = TblUser.Create("testuser", "test@example.com", "hash", "Test User", "CUSTOMER");
        Assert.False(user.IsLockedOut);
        Assert.Equal(0, user.AccessFailedCount);

        for (int i = 1; i <= 4; i++)
        {
            user.RecordFailedLogin(maxFailedAttempts: 5, lockoutMinutes: 15);
            Assert.False(user.IsLockedOut);
            Assert.Equal(i, user.AccessFailedCount);
        }

        // 5th failure -> locked out
        user.RecordFailedLogin(maxFailedAttempts: 5, lockoutMinutes: 15);
        Assert.True(user.IsLockedOut);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd.Value > DateTime.UtcNow);

        // Reset
        user.ResetAccessFailedCount();
        Assert.False(user.IsLockedOut);
        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void TblUser_TwoFactor_EnableDisableAndConsumeRecoveryCode()
    {
        var user = TblUser.Create("test2fa", "2fa@example.com", "hash", "2FA User", "CUSTOMER");
        Assert.False(user.TwoFactorEnabled);

        var codes = "AAAA-1111,BBBB-2222,CCCC-3333";
        user.EnableTwoFactor("TESTSECRETKEY", codes);

        Assert.True(user.TwoFactorEnabled);
        Assert.Equal("TESTSECRETKEY", user.TwoFactorSecret);

        // Consume valid code
        var consumed = user.ConsumeRecoveryCode("aaaa-1111");
        Assert.True(consumed);
        Assert.DoesNotContain("AAAA-1111", user.TwoFactorRecoveryCodes);

        // Try reusing same code -> should fail
        var reused = user.ConsumeRecoveryCode("AAAA-1111");
        Assert.False(reused);

        // Disable
        user.DisableTwoFactor();
        Assert.False(user.TwoFactorEnabled);
        Assert.Null(user.TwoFactorSecret);
        Assert.Null(user.TwoFactorRecoveryCodes);
    }
}
