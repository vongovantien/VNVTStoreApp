namespace VNVTStore.Domain.Interfaces;

/// <summary>
/// Unit of Work Interface - đơn giản hóa, chỉ giữ CommitAsync
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default, params object[] parameters);
    void ClearChangeTracker();
    bool IsNew<T>(T entity) where T : class;
}
