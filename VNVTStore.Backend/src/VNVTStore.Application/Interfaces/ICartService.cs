using VNVTStore.Application.Common;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface ICartService
{
    Task<TblCart> GetOrCreateCartAsync(string userCode, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string userCode, CancellationToken cancellationToken = default);
}
