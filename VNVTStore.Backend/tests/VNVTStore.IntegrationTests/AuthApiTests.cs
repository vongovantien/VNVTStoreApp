using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Enums;
using Xunit;

namespace VNVTStore.IntegrationTests;

public class AuthApiTests : IntegrationTestBase
{
    public AuthApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Impersonate_AsAdmin_ShouldReturnSuccess()
    {
        // 0. Ensure target user exists (webuser from seed)
        // In a real test, we'd query the DB via factory.Services
        string targetUserCode = "webuser"; 

        // Actually, let's try to find a user first via a generic user list if exists, 
        // or just use the seeded "webuser".
        
        // Act
        await AuthenticateAsync("admin", "Admin@123");
        var response = await _client.PostAsync($"/api/v1/auth/impersonate/{targetUserCode}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Token.Should().NotBeNullOrEmpty();
        content.Data.User.Username.Should().Be("webuser");
    }

    [Fact]
    public async Task Impersonate_AsNonAdmin_ShouldReturnForbidden()
    {
        // 1. Authenticate as regular staff/user
        await AuthenticateAsync("webuser", "Staff@123");

        // 2. Try to impersonate someone else
        var response = await _client.PostAsync("/api/v1/auth/impersonate/admin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
