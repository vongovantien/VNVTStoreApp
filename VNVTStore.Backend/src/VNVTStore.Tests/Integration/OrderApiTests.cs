using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VNVTStore.API;

namespace VNVTStore.Tests.Integration;

public class OrderApiTests : ApiTestBase
{
    public OrderApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetOrders_ReturnsSuccessOrUnauthorized()
    {
        var response = await Client.GetAsync("/api/v1/orders");
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}
