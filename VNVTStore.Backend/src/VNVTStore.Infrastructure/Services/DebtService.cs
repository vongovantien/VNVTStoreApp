using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class DebtService : IDebtService
{
    private readonly IRepository<TblUser> _userRepo;
    private readonly IRepository<TblDebtLog> _debtLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DebtService(
        IRepository<TblUser> userRepo, 
        IRepository<TblDebtLog> debtLogRepo, 
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _userRepo = userRepo;
        _debtLogRepo = debtLogRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<decimal> GetCurrentBalanceAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByCodeAsync(userCode, cancellationToken);
        return user?.CurrentDebt ?? 0;
    }

    public async Task<Result<bool>> CheckDebtLimitAsync(string userCode, decimal additionalAmount, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByCodeAsync(userCode, cancellationToken);
        if (user == null) return Result.Failure<bool>(new Error("UserNotFound", "User not found"));

        if (user.CurrentDebt + additionalAmount > user.DebtLimit)
        {
            return Result.Failure<bool>(new Error("DebtLimitExceeded", 
                $"Debt limit exceeded. Current: {user.CurrentDebt:N0}, Limit: {user.DebtLimit:N0}, Requested: {additionalAmount:N0}"));
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> RecordDebtChangeAsync(string userCode, decimal amount, string reason, string? orderCode = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByCodeAsync(userCode, cancellationToken);
        if (user == null) return Result.Failure<bool>(new Error("UserNotFound", "User not found"));

        var balanceBefore = user.CurrentDebt;
        user.UpdateDebt(amount);
        
        var debtLog = new TblDebtLog
        {
            UserCode = userCode,
            OrderCode = orderCode,
            Amount = amount,
            Reason = reason,
            BalanceAfter = user.CurrentDebt,
            RecordedBy = _currentUser.UserCode ?? "System",
            CreatedAt = DateTime.UtcNow
        };

        await _debtLogRepo.AddAsync(debtLog, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
