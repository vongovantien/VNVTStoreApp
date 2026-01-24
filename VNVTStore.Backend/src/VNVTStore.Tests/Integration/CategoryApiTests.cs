using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VNVTStore.API;
using System.Net;
using System.Collections.Generic;

namespace VNVTStore.Tests.Integration;

public class CategoryApiTests : ApiTestBase
{
    public CategoryApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ShouldNotReturnServerError()
    {
        var response = await Client.GetAsync("/api/v1/categories");
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidAccess_ShouldNotBeSuccessful()
    {
        var dto = new { Name = "Test Category" };
        var response = await Client.PostAsJsonAsync("/api/v1/categories", dto);
        
        // Assert that unauthenticated creation is blocked
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest);
    }
}
