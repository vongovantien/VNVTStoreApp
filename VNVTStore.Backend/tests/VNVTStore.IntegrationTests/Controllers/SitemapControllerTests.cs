using System.Net;
using System.Threading.Tasks;
using Xunit;
using VNVTStore.API;
using System.Net.Http;

namespace VNVTStore.IntegrationTests;

public class SitemapControllerTests : IntegrationTestBase
{
    public SitemapControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetSitemap_ReturnsSuccessAndValidXml()
    {
        // Act
        var response = await _client.GetAsync("/sitemap.xml");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("<?xml", content);
        Assert.Contains("<urlset", content);
        Assert.Contains("<url>", content);
        Assert.Contains("<loc>", content);
    }
}
