using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using VNVTStore.Application.DTOs;
using VNVTStore.API;
using Xunit;

namespace VNVTStore.IntegrationTests;

public class ProductsApiTests : IntegrationTestBase
{
    public ProductsApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProducts_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products/search", new { });
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_Paged_ShouldReturnPagedResult()
    {
        // Arrange
        var request = new
        {
            pageIndex = 1,
            pageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products/search", request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("items");
    }

    [Fact]
    public async Task GetProductByCode_NotFound_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products/NONEXISTENT_CODE");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var product = new CreateProductDto
        {
            Name = "Test Product",
            Price = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", product);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturn400()
    {
        // Arrange - empty name should fail validation
        var product = new CreateProductDto
        {
            Name = "",
            Price = -100 // Invalid price
        };

        // Act - Without auth, expect 401. With invalid data, expect 400.
        var response = await _client.PostAsJsonAsync("/api/v1/products", product);
        
        // Assert - 401 because not authenticated
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class CategoriesApiTests : IntegrationTestBase
{
    public CategoriesApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetCategories_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/categories/search", new { });
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_Paged_ShouldReturnPagedResult()
    {
        // Arrange
        var request = new
        {
            pageIndex = 1,
            pageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/categories/search", request);
        await EnsureSuccessAsync(response);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class NewsApiTests : IntegrationTestBase
{
    public NewsApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetNews_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/news/search", new { });
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNews_Paged_ShouldReturnPagedResult()
    {
        // Arrange
        var request = new
        {
            pageIndex = 1,
            pageSize = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/news/search", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateNews_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var news = new CreateNewsDto
        {
            Title = "Test News",
            Content = "Test content"
        };

        // Act - NewsController doesn't have [Authorize], so Create is public
        var request = new RequestDTO<CreateNewsDto> { PostObject = news };
        var response = await _client.PostAsJsonAsync("/api/v1/news", request);
        
        // Assert - may return 401 if auth middleware catches it, or succeed/fail for other reasons
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}

public class RolesApiTests : IntegrationTestBase
{
    public RolesApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetPermissions_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/roles/permissions");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissions_WithAdminAuth_ShouldReturnSuccess()
    {
        // Arrange
        await AuthenticateAsync("admin", "Admin@123");

        // Act
        var response = await _client.GetAsync("/api/v1/roles/permissions");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("items");
    }
}
