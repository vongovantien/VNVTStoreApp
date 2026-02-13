using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;
using Xunit;
using System.Net.Http.Json;

namespace VNVTStore.IntegrationTests;

public class AuditLogsApiTests : IntegrationTestBase
{
    public AuditLogsApiTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchAuditLogs_Admin_ShouldReturnSuccess()
    {
        // Arrange
        await AuthenticateAsync("admin", "Admin@123"); // Matching DataSeeder
        var searchRequest = new SearchParams
        {
            PageSize = 10,
            PageIndex = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auditlogs/search", searchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AuditLogDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAuditLogs_Anonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        // No authentication

        var searchRequest = new SearchParams
        {
            PageSize = 10,
            PageIndex = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auditlogs/search", searchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
