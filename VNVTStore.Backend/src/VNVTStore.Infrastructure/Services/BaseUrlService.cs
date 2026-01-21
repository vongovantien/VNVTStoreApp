using Microsoft.AspNetCore.Http;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class BaseUrlService : IBaseUrlService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BaseUrlService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return "";

        return $"{request.Scheme}://{request.Host}";
    }
}
