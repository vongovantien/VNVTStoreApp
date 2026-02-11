using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.DTOs;
using VNVTStore.API;
using VNVTStore.Application.Common; // For ApiResponse
using Xunit;

namespace VNVTStore.IntegrationTests;

public class PartialSelectionTests : IntegrationTestBase
{
    public PartialSelectionTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProducts_WithPartialChildSelection_ShouldReturnOnlyRequestedFields()
    {
        Console.WriteLine("[DEBUG] Authenticating...");
        await AuthenticateAsync("admin", "Admin@123");
        Console.WriteLine("[DEBUG] Authenticated.");

        // 2. Create a Product with Images
        var createDto = new CreateProductDto
        {
            Name = "Partial Selection Test Product",
            Price = 1000,
            Images = new List<string> { "http://example.com/image1.jpg", "http://example.com/image2.jpg" }
        };

        Console.WriteLine("[DEBUG] Creating product...");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createDto);
        Console.WriteLine($"[DEBUG] Create Response Status: {createResponse.StatusCode}");
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created); 
        
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productCode = createdProduct!.Data!.Code;
        Console.WriteLine($"[DEBUG] Created Product Code: {productCode}");

        // 3. Request Product with Partial Fields (Name, ProductImages.ImageURL)
        var searchRequest = new
        {
            pageIndex = 1,
            pageSize = 10,
            Searching = new List<SearchDTO> 
            { 
                new SearchDTO { SearchField = "Code", SearchValue = productCode, SearchCondition = SearchCondition.Equal } 
            },
            fields = new List<string> { "Name", "ProductImages.ImageURL" } 
        };

        Console.WriteLine("[DEBUG] Searching for product with partial fields...");
        var response = await _client.PostAsJsonAsync("/api/v1/products/search", searchRequest); 
        Console.WriteLine($"[DEBUG] Search Response Status: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        Console.WriteLine($"[DEBUG] Paged Result Success: {pagedResult?.Success}");
        pagedResult.Should().NotBeNull();
        pagedResult!.Data.Should().NotBeNull();
        pagedResult.Data!.Items.Should().HaveCount(1);
        var item = pagedResult.Data.Items.First();
        Console.WriteLine($"[DEBUG] Item Name: {item.Name}, Images Count: {item.ProductImages?.Count}");

        // 4. Verify Main Property
        item.Name.Should().Be("Partial Selection Test Product");
        item.Price.Should().Be(0); // Should be default (0) because it wasn't requested

        // 5. Verify Child Collection
        item.ProductImages.Should().NotBeNullOrEmpty();
        var image = item.ProductImages.First();
        
        // ImageURL should be populated
        image.ImageURL.Should().NotBeNullOrEmpty();
        
        // Other fields like AltText/SortOrder should be null/default
        image.AltText.Should().BeNull();
        image.SortOrder.Should().BeNull();
    }
}
