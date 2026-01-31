namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Interface for caching service - supports both Memory and Redis cache
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove all keys matching pattern (for invalidation)
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get or create cached value
    /// </summary>
    Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<CancellationToken, Task<T>> factory, 
        TimeSpan? absoluteExpiration = null, 
        CancellationToken cancellationToken = default);
}
