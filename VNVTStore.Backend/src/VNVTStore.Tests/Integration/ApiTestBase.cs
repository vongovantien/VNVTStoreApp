using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http;
using VNVTStore.API;

namespace VNVTStore.Tests.Integration;

public class ApiTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;

    public ApiTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
