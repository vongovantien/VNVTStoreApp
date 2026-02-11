using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.API;

namespace VNVTStore.IntegrationTests;

public class ProductStatusTests : IntegrationTestBase
{
    public ProductStatusTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateProductStatus_ShouldPersistChange()
    {
        // 1. Authenticate
        await AuthenticateAsync();

        // 2. Create a product
        var createProductDto = new CreateProductDto
        {
            Name = "Test Status Product",
            Price = 100000,
            CategoryCode = "CAT001",
            StockQuantity = 10,
            CostPrice = 80000,
            IsNew = true
        };
        
        var createRequest = new RequestDTO<CreateProductDto> { PostObject = createProductDto };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        createResult!.Success.Should().BeTrue();
        var productCode = createResult.Data!.Code;
        // createResult.Data.IsActive.Should().BeTrue(); // Default should be true

        // 3. Update the product status to Inactive
        var updateProductDto = new UpdateProductDto
        {
            IsActive = false
        };
        
        var updateRequest = new RequestDTO<UpdateProductDto> { PostObject = updateProductDto };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/products/{productCode}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        updateResult!.Success.Should().BeTrue();
        // The DTO returned after update should reflect the new status
        updateResult.Data!.IsActive.Should().BeFalse("Update response should reflect the new isActive status");

        // 4. Verify by fetching the product again
        var getResponse = await _client.GetAsync($"/api/v1/products/{productCode}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        getResult!.Success.Should().BeTrue();
        getResult.Data!.IsActive.Should().BeFalse("Database should persist the isActive status as false");
    }
}
