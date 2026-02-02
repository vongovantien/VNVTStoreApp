using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

public interface IDebtService
{
    /// <summary>
    /// Checks if a user has exceeded their debt limit or would exceed it with the new amount.
    /// </summary>
    Task<Result<bool>> CheckDebtLimitAsync(string userCode, decimal additionalAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a change in user debt (increase from order or decrease from payment).
    /// </summary>
    Task<Result<bool>> RecordDebtChangeAsync(string userCode, decimal amount, string reason, string? orderCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current debt balance for a user.
    /// </summary>
    Task<decimal> GetCurrentBalanceAsync(string userCode, CancellationToken cancellationToken = default);
}
