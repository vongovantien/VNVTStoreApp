using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VNVTStore.API;
using System.Net;

namespace VNVTStore.Tests.Integration;

public class DashboardApiTests : ApiTestBase
{
    public DashboardApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetStats_ReturnsSuccessOrAuth()
    {
        // Try both possible routes based on controller naming
        var response = await Client.GetAsync("/api/v1/warehouse/dashboard-stats");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response = await Client.GetAsync("/api/v1/warehouse/stats");
        }
        
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        // We verify the route exists or is secured
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden);
    }
}
