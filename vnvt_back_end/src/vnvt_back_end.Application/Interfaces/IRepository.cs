using System.Linq.Expressions;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Basic CRUD Operations
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
        //Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);

        IQueryable<T> Where(Expression<Func<T, bool>> predicate);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Bulk Operations
        Task AddRangeAsync(IEnumerable<T> entities);
        void UpdateRange(IEnumerable<T> entities);
        void DeleteRange(IEnumerable<T> entities);

        // Querying with Projections
        Task<IEnumerable<TResult>> GetProjectedAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult> GetFirstProjectedAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate);

        // Pagination
        Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null);

        // Sorting
        IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        // Grouping
        IQueryable<IGrouping<TKey, T>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);
        Task<IEnumerable<IGrouping<TKey, T>>> GetGroupedAsync<TKey>(Expression<Func<T, TKey>> keySelector);

        // Relationships
        IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties);

        // Transactions
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Soft Deletes
        Task SoftDeleteAsync(T entity);
        IQueryable<T> GetDeleted();
        Task RestoreDeletedAsync(T entity);

        // Auditing
        Task LogChangesAsync(string changeDetails);

        // Advanced Queries
        Task<IEnumerable<T>> ExecuteSqlQueryAsync(string sqlQuery, params object[] parameters);
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string storedProcedure, params object[] parameters);

        // Miscellaneous
        Task<int> ExecuteCommandAsync(string command, params object[] parameters);
        Task<IEnumerable<T>> GetByCustomQueryAsync(string customQuery, params object[] parameters);
        Task<T> GetByKeyAsync(params object[] keyValues);
    }

}
