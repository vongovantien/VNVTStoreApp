using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.API;

namespace VNVTStore.IntegrationTests;

public class PromotionsTests : IntegrationTestBase
{
    public PromotionsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPromotions_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/promotions/search", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<PromotionDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePromotion_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var createPromoDto = new CreatePromotionDto
        {
            Code = "TESTPROM" + Guid.NewGuid().ToString("N").Substring(0, 5),
            Name = "Test Integration Promotion",
            DiscountType = "PERCENTAGE",
            DiscountValue = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        
        var request = new RequestDTO<CreatePromotionDto> { PostObject = createPromoDto };
        await AuthenticateAsync("admin", "Admin@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/promotions", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PromotionDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(createPromoDto.Code);
    }

    [Fact]
    public async Task CreatePromotion_WithInvalidDiscountType_ShouldReturnError()
    {
        // Arrange
        var createPromoDto = new CreatePromotionDto
        {
            Code = "INVALIDPROM" + Guid.NewGuid().ToString("N").Substring(0, 5),
            Name = "Invalid Type Promo",
            DiscountType = "INVALID_TYPE", // This should trigger the DB constraint if configured
            DiscountValue = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };

        var request = new RequestDTO<CreatePromotionDto> { PostObject = createPromoDto };
        await AuthenticateAsync("admin", "Admin@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/promotions", request);

        // Assert
        // If there's a DB constraint, it might return 500 or 400 depending on how exceptions are handled
        // Given the user reported a constraint violation error, it likely reaches the DB
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}
