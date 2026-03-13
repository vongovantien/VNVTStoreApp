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
        // 1. Get a product first to have a valid product code
        var productsResponse = await _client.PostAsJsonAsync("/api/v1/products/search", new { pageIndex = 1, pageSize = 1 });
        var productsData = await productsResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        
        productsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        productsData.Should().NotBeNull();
        productsData?.Data.Should().NotBeNull();
        productsData?.Data?.Items.Should().NotBeNullOrEmpty();
        
        var productCode = productsData!.Data!.Items!.First().Code;
        
        // 2. Act
        await AuthenticateAsync("admin", "Admin@123");
        var response = await _client.GetAsync($"/api/v1/reviews/product/{productCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ReviewDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}
