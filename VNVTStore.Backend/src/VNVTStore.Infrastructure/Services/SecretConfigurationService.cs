using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Infrastructure.Services;

public class SecretConfigurationService : ISecretConfigurationService
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public SecretConfigurationService(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<string?> GetSecretAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var cacheKey = $"Secret_{key}";
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        var secret = await _context.TblSystemSecrets.FirstOrDefaultAsync(s => s.Code == key && s.IsActive);
        if (secret != null && secret.SecretValue != null)
        {
            _cache.Set(cacheKey, secret.SecretValue, CacheDuration);
            return secret.SecretValue;
        }

        return null; // or throw depending on how strict we want it
    }

    public async Task SetSecretAsync(string key, string value, string? description = null)
    {
        var secret = await _context.TblSystemSecrets.FirstOrDefaultAsync(s => s.Code == key);
        if (secret == null)
        {
            secret = new TblSystemSecret
            {
                Code = key,
                SecretValue = value,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.TblSystemSecrets.Add(secret);
        }
        else
        {
            secret.SecretValue = value;
            if (description != null)
                secret.Description = description;
                
            secret.UpdatedAt = DateTime.UtcNow;
            _context.TblSystemSecrets.Update(secret);
        }

        await _context.SaveChangesAsync(default);
        ClearCache(key);
    }

    public async Task DeleteSecretAsync(string key)
    {
        var secret = await _context.TblSystemSecrets.FirstOrDefaultAsync(s => s.Code == key);
        if (secret != null)
        {
            _context.TblSystemSecrets.Remove(secret);
            await _context.SaveChangesAsync(default);
            ClearCache(key);
        }
    }

    public async Task<bool> HasSecretAsync(string key)
    {
        var cacheKey = $"Secret_{key}";
        if (_cache.TryGetValue(cacheKey, out _))
            return true;
            
        return await _context.TblSystemSecrets.AnyAsync(s => s.Code == key && s.IsActive);
    }

    public void ClearCache(string? key = null)
    {
        if (key != null)
        {
            _cache.Remove($"Secret_{key}");
        }
        else
        {
            // Standard IMemoryCache does not support bulk clear by prefix easily.
            // Usually we'd use a CancellationTokenSource to reset cache regions, but for simple use case, 
            // the admin operations just clear the specific keys they touch.
            // If they need to clear all, they'd have to wait for expiry or restart app.
        }
    }
}
