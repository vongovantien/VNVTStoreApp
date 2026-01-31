using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

/// <summary>
/// In-memory cache implementation using IMemoryCache
/// Suitable for single-instance deployments
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _cache.Get<T>(key);
        if (value != null)
        {
            _logger.LogDebug("Cache HIT for key: {Key}", key);
        }
        else
        {
            _logger.LogDebug("Cache MISS for key: {Key}", key);
        }
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        _logger.LogDebug("Cache SET for key: {Key}, Expiration: {Expiration}", key, absoluteExpiration ?? _defaultExpiration);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("Cache REMOVE for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
        
        _logger.LogDebug("Cache REMOVE by prefix: {Prefix}, Count: {Count}", prefix, keysToRemove.Count);
        return Task.CompletedTask;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<CancellationToken, Task<T>> factory, 
        TimeSpan? absoluteExpiration = null, 
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        if (value != null)
        {
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
        }
        
        return value;
    }
}

/// <summary>
/// Cache key constants for consistency
/// </summary>
public static class CacheKeys
{
    public const string Products = "products";
    public const string ProductByCode = "product:{0}";
    public const string Categories = "categories";
    public const string CategoryByCode = "category:{0}";
    public const string Brands = "brands";
    public const string Banners = "banners";
    public const string ActivePromotions = "promotions:active";
    
    public static string ForProduct(string code) => string.Format(ProductByCode, code);
    public static string ForCategory(string code) => string.Format(CategoryByCode, code);
}
