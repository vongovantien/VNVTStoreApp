using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Text;
using System.Xml;
using VNVTStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("sitemap.xml")]
public class SitemapController : ControllerBase
{
    private readonly IDapperContext _dapperContext;
    private const string SITE_URL = "https://vnvtstore.com";

    public SitemapController(IDapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSitemap()
    {
        var sitemapContent = await GenerateSitemapXml();
        return Content(sitemapContent, "application/xml", Encoding.UTF8);
    }

    private async Task<string> GenerateSitemapXml()
    {
        using var connection = _dapperContext.CreateConnection();
        
        // Fetch active products
        var productCodes = await connection.QueryAsync<string>(
            @"SELECT ""Code"" FROM ""TblProduct"" WHERE ""IsActive"" = true"
        );

        // Fetch active news
        var newsCodes = await connection.QueryAsync<string>(
            @"SELECT ""Code"" FROM ""TblNews"" WHERE ""IsActive"" = true"
        );

        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

        // Static routes
        var staticPages = new[] { "", "/products", "/news", "/about", "/contact", "/support", "/promotions" };
        foreach (var page in staticPages)
        {
            AddUrl(sb, $"{SITE_URL}{page}", "1.0", "daily");
        }

        // Product routes
        foreach (var code in productCodes)
        {
            AddUrl(sb, $"{SITE_URL}/product/{code}", "0.8", "weekly");
        }

        // News routes
        foreach (var code in newsCodes)
        {
            AddUrl(sb, $"{SITE_URL}/news/{code}", "0.6", "weekly");
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    private void AddUrl(StringBuilder sb, string url, string priority, string changefreq)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{url}</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }
}
