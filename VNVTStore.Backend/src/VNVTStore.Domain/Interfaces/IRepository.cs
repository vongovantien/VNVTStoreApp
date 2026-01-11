using System.Linq.Expressions;

namespace VNVTStore.Domain.Interfaces;

/// <summary>
/// Generic Repository Interface - dùng Code (string) làm primary key
/// </summary>
public interface IRepository<T> where T : class
{
    // Basic CRUD với Code-based key
    Task<T?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    // Query operations
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    IQueryable<T> AsQueryable();
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
