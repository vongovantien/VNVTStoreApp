using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.API;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        return await _client.GetFromJsonAsync<T>(url);
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        return await _client.PostAsJsonAsync(url, data);
    }
    
    // Helper to get a service from the test server
    protected T GetService<T>() where T : notnull
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task AuthenticateAsync(string username = "admin", string password = "password")
    {
        var loginRequest = new 
        { 
            Username = username, 
            Password = password 
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (content?.Data != null)
            {
                _client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", content.Data.Token);
            }
        }
    }
}
