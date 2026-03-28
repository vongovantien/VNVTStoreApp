using System.Threading.Tasks;

namespace VNVTStore.Application.Interfaces;

public interface ISecretConfigurationService
{
    Task<string?> GetSecretAsync(string key);
    Task SetSecretAsync(string key, string value, string? description = null);
    Task DeleteSecretAsync(string key);
    Task<bool> HasSecretAsync(string key);
    void ClearCache(string? key = null);
}
