using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.API;

namespace VNVTStore.IntegrationTests;

public class ReviewApiTests : IntegrationTestBase
{
    public ReviewApiTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProductReviews_ShouldReturnSuccess()
    {
        // Arrange
        var productCode = "afb6e5ee39"; // Using the code from the user's screenshot
        
        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productCode}/reviews");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ReviewDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        
        // Assert Pagination Metadata
        result.Data.Should().NotBeNull();
        result.Data!.PageIndex.Should().BeGreaterOrEqualTo(1);
        result.Data.PageSize.Should().BeGreaterThan(0);
        result.Data.TotalPages.Should().BeGreaterOrEqualTo(0);
        // We can't strictly assert TotalPages > 0 because if there are no reviews, it might be 0 or 1 depending on logic
        // But checking fields exist is enough to verify DTO change.
    }
}
