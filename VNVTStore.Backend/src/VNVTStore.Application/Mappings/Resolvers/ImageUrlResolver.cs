using AutoMapper;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Mappings.Resolvers;

public class ImageUrlResolver : IMemberValueResolver<object, object, string?, string?>
{
    private readonly IBaseUrlService _baseUrlService;

    public ImageUrlResolver(IBaseUrlService baseUrlService)
    {
        _baseUrlService = baseUrlService;
    }

    public string? Resolve(object source, object destination, string? sourceMember, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(sourceMember)) return null;
        
        // If it's already an absolute URL, return it
        if (sourceMember.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return sourceMember;

        var baseUrl = _baseUrlService.GetBaseUrl();
        if (string.IsNullOrEmpty(baseUrl)) return sourceMember;

        // Ensure slash separator
        var relativePath = sourceMember.Replace('\\', '/').TrimStart('/');
        return $"{baseUrl}/{relativePath}";
    }
}
