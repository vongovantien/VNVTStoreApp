using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Infrastructure.Services;

/// <summary>
/// Redis cache implementation for multi-instance deployments
/// Configure via CacheSettings:Redis:ConnectionString in appsettings.json
/// </summary>
public class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly bool _isEnabled;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetValue<string>("CacheSettings:Redis:ConnectionString");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("[Constructor] error: Redis connection string not configured. Redis caching is disabled.");
            _isEnabled = false;
            return;
        }
        
        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
            _isEnabled = true;
            _logger.LogInformation("[Constructor] Redis cache connected successfully to {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Constructor] error: Failed to connect to Redis. Redis caching is disabled.");
            _isEnabled = false;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _db == null) return default;
        
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("[GetAsync] Redis MISS for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("[GetAsync] Redis HIT for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetAsync] error: Redis GET failed for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _db == null) return;
        
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var expiry = absoluteExpiration ?? _defaultExpiration;
            await _db.StringSetAsync(key, json, expiry);
            _logger.LogDebug("[SetAsync] Redis SET for key: {Key}, Expiration: {Expiration}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SetAsync] error: Redis SET failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _db == null) return;
        
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogDebug("[RemoveAsync] Redis REMOVE for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemoveAsync] error: Redis REMOVE failed for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _redis == null || _db == null) return;
        
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keysToRemove = server.Keys(pattern: $"{prefix}*").ToArray();
            
            if (keysToRemove.Length > 0)
            {
                await _db.KeyDeleteAsync(keysToRemove);
                _logger.LogDebug("[RemoveByPrefixAsync] Redis REMOVE by prefix: {Prefix}, Count: {Count}", prefix, keysToRemove.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemoveByPrefixAsync] error: Redis REMOVE by prefix failed for: {Prefix}", prefix);
        }
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

    public void Dispose()
    {
        _redis?.Dispose();
    }
}
