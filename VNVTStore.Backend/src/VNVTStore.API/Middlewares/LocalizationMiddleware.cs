using System.Globalization;

namespace VNVTStore.API.Middlewares;

/// <summary>
/// Middleware to set culture based on Accept-Language header
/// </summary>
public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _supportedCultures = { "vi", "en" };
    private const string DefaultCulture = "vi";

    public LocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        var culture = GetCultureFromHeader(acceptLanguage);
        
        // Set culture for the current request
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        
        // Store in HttpContext.Items for easy access in handlers
        context.Items["Culture"] = culture;
        context.Items["Language"] = culture;

        await _next(context);
    }

    private string GetCultureFromHeader(string? acceptLanguage)
    {
        if (string.IsNullOrEmpty(acceptLanguage))
            return DefaultCulture;

        // Parse Accept-Language header (e.g., "vi", "en-US", "vi-VN,vi;q=0.9,en;q=0.8")
        var languages = acceptLanguage.Split(',')
            .Select(lang => lang.Split(';').First().Trim())
            .Select(lang => lang.Split('-').First().ToLower())
            .ToList();

        foreach (var lang in languages)
        {
            if (_supportedCultures.Contains(lang))
                return lang;
        }

        return DefaultCulture;
    }
}

/// <summary>
/// Extension method to add localization middleware
/// </summary>
public static class LocalizationMiddlewareExtensions
{
    public static IApplicationBuilder UseLanguageLocalization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LocalizationMiddleware>();
    }
}
