using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.API;
using Xunit;

namespace VNVTStore.IntegrationTests;

public class PromotionDebugTests : IntegrationTestBase
{
    public PromotionDebugTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePromotion_Debug()
    {
        // Arrange
        var createPromoDto = new CreatePromotionDto
        {
            Code = "DEBUGPROM" + Guid.NewGuid().ToString("N").Substring(0, 5),
            Name = "Debug Integration Promotion",
            DiscountType = "PERCENTAGE",
            DiscountValue = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        
        var request = new RequestDTO<CreatePromotionDto> { PostObject = createPromoDto };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/promotions", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Test failed with status {response.StatusCode}. Content: {content}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
