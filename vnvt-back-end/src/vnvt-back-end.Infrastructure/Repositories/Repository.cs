using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure.Extensions;

namespace vnvt_back_end.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;
        private IDbContextTransaction _transaction;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }



        // Basic CRUD Operations
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Bulk Operations
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            _context.SaveChanges();
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }

        // Querying with Projections
        public async Task<IEnumerable<TResult>> GetProjectedAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Select(selector).ToListAsync();
        }

        public async Task<TResult> GetFirstProjectedAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Select(selector).FirstOrDefaultAsync();
        }

        // Pagination
        public async Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                                   .Take(pagingParameters.PageSize)
                                   .ToListAsync();

            return new PagedResult<T>(items, totalRecords, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        // Sorting
        public IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return _dbSet.OrderBy(keySelector);
        }

        public IQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return _dbSet.OrderByDescending(keySelector);
        }

        // Grouping
        public IQueryable<IGrouping<TKey, T>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return _dbSet.GroupBy(keySelector);
        }

        public async Task<IEnumerable<IGrouping<TKey, T>>> GetGroupedAsync<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return await _dbSet.GroupBy(keySelector).ToListAsync();
        }

        // Relationships
        public IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        // Transactions
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                _transaction.Dispose();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
            }
        }

        // Soft Deletes
        public async Task SoftDeleteAsync(T entity)
        {
            // Assuming that entity has a property `IsDeleted`
            var entry = _context.Entry(entity);
            entry.Property("IsDeleted").CurrentValue = true;
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetDeleted()
        {
            // Assuming that entity has a property `IsDeleted`
            return _dbSet.Where(e => EF.Property<bool>(e, "IsDeleted"));
        }

        public async Task RestoreDeletedAsync(T entity)
        {
            var entry = _context.Entry(entity);
            entry.Property("IsDeleted").CurrentValue = false;
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Auditing
        public async Task LogChangesAsync(string changeDetails)
        {
            // Implement logging logic here
            await Task.CompletedTask;
        }

        // Advanced Queries
        public async Task<IEnumerable<T>> ExecuteSqlQueryAsync(string sqlQuery, params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sqlQuery, parameters).ToListAsync();
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string storedProcedure, params object[] parameters)
        {
            return await _context.Set<T>().FromSqlRaw(storedProcedure, parameters).ToListAsync();
        }

        // Miscellaneous
        public async Task<int> ExecuteCommandAsync(string command, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(command, parameters);
        }

        public async Task<IEnumerable<T>> GetByCustomQueryAsync(string customQuery, params object[] parameters)
        {
            return await _context.Set<T>().FromSqlRaw(customQuery, parameters).ToListAsync();
        }

        public async Task<T> GetByKeyAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }
        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes)

        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (!string.IsNullOrEmpty(pagingParameters.Keyword))
            {
                var parameter = Expression.Parameter(typeof(T), "p");
                var property = Expression.Property(parameter, "Name");
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var someValue = Expression.Constant(pagingParameters.Keyword, typeof(string));
                var containsExpression = Expression.Call(property, containsMethod, someValue);

                var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

                query = query.Where(lambda);
            }

            if (!string.IsNullOrEmpty(pagingParameters.SortField))
            {
                query = pagingParameters.SortDescending
                    ? query.OrderByDescending(pagingParameters.SortField)
                    : query.OrderBy(pagingParameters.SortField);
            }

            var totalItems = await query.CountAsync();
            var items = await query.Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                                   .Take(pagingParameters.PageSize)
                                   .ToListAsync();

            return new PagedResult<T>(items, totalItems, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
    }
}
