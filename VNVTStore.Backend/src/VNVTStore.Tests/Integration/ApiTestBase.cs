using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http;
using VNVTStore.API;

namespace VNVTStore.Tests.Integration;

public class ApiTestBase : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory<Program> Factory;

    public ApiTestBase(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
