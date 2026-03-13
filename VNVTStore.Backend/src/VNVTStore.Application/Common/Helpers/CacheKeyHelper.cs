using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace VNVTStore.Application.Common.Helpers;

public static class CacheKeyHelper
{
    public static string GenerateKeyFromRequest(string prefix, object request)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None // Minimized JSON
        };

        var json = JsonConvert.SerializeObject(request, settings);
        using var hash = MD5.Create(); // MD5 is fast enough for cache keys (non-crypto)
        // Or SHA256
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        
        return $"{prefix}_{sb}";
    }
}
