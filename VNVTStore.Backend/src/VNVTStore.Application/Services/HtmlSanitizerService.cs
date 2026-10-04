using Ganss.Xss;

namespace VNVTStore.Application.Services;

/// <summary>
/// HTML Sanitizer Service - Ngăn XSS attacks
/// Sanitize tất cả user-generated content (reviews, descriptions, comments)
/// </summary>
public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
    string SanitizeForDisplay(string? html);
}

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;
    private readonly HtmlSanitizer _strictSanitizer;

    public HtmlSanitizerService()
    {
        // Sanitizer cho admin input (cho phép basic formatting)
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.AllowedAttributes.Add("target");
        
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Strict sanitizer cho user reviews (chỉ plain text + basic formatting)
        _strictSanitizer = new HtmlSanitizer();
        _strictSanitizer.AllowedTags.Clear();
        _strictSanitizer.AllowedTags.Add("p");
        _strictSanitizer.AllowedTags.Add("br");
        _strictSanitizer.AllowedTags.Add("strong");
        _strictSanitizer.AllowedTags.Add("em");
        
        _strictSanitizer.AllowedAttributes.Clear();
    }

    /// <summary>
    /// Sanitize HTML cho admin content (product descriptions, etc.)
    /// Cho phép basic HTML tags
    /// </summary>
    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Sanitize(html);
    }

    /// <summary>
    /// Sanitize HTML cho user-generated content (reviews, comments)
    /// Chỉ cho phép plain text + minimal formatting
    /// </summary>
    public string SanitizeForDisplay(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _strictSanitizer.Sanitize(html);
    }
}
