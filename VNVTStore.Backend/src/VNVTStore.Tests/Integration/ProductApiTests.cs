using System;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using VNVTStore.API;
using VNVTStore.API.Controllers.v1;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Tests.Integration;

public class ProductApiTests : ApiTestBase
{
    public ProductApiTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<string> AuthenticateAsync(bool isAdmin = false)
    {
        var uniqueSub = Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"prod_test_{uniqueSub}@example.com";
        var password = "Password123!";
        var registerRequest = new RegisterRequest($"u_{uniqueSub}", email, password, "Test User");

        // 1. Register
        var regResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        regResponse.EnsureSuccessStatusCode();

        // 2. Promote to Admin if needed
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblUser>>();
            var user = await userRepo.FindAsync(u => u.Email == email);
            if (user == null) throw new Exception("Registered user not found in DB");
            
            // Verify email to allow login if required (default is false)
            if (!string.IsNullOrEmpty(user.EmailVerificationToken))
            {
                user.VerifyEmail(user.EmailVerificationToken);
            }
            
            if (isAdmin)
            {
                user.UpdateRole("ADMIN");
            }
            userRepo.Update(user);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        // 3. Login
        var loginRequest = new LoginRequest($"u_{uniqueSub}", password);
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var authResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        if (authResult == null || authResult.Data == null) throw new Exception("Login failed to return token");
        return authResult!.Data!.Token;
    }

    private async Task<string> CreateTestCategoryAsync()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var catRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblCategory>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var catCode = $"CAT_{Guid.NewGuid().ToString("N").Substring(0,5)}";
            var category = new TblCategory 
            { 
                Code = catCode, 
                Name = $"Test Category {catCode}", 
                IsActive = true 
            };
            
            await catRepo.AddAsync(category);
            await unitOfWork.CommitAsync();
            
            return catCode;
        }
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnSuccess()
    {
            // Arrange
            var token = await AuthenticateAsync(isAdmin: true);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var categoryCode = await CreateTestCategoryAsync();
            
            var productDto = new CreateProductDto
            {
                Name = "Integration Test Product",
                Price = 150000,
                StockQuantity = 100,
                CategoryCode = categoryCode,
                BaseUnit = "Cái",
                Description = "Created via Integration Test",
                IsNew = true
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/products", new RequestDTO<CreateProductDto> { PostObject = productDto });
            
            if (response.StatusCode != HttpStatusCode.OK) {
                 var errorContent = await response.Content.ReadAsStringAsync();
                 throw new Exception($"API ERROR: {response.StatusCode} - {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            
            if (result != null && !result.Success)
            {
                 throw new Exception($"API LOGIC ERROR: {result.Message}");
            }

            Assert.NotNull(result);
            Assert.True(result!.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(productDto.Name, result.Data!.Name);
            Assert.Equal(productDto.Price, result.Data!.CostPrice ?? result.Data!.WholesalePrice ?? 150000); 
    }
}
