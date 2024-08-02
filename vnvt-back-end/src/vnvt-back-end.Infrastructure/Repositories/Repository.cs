using Microsoft.EntityFrameworkCore;
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

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public async Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

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
                // Assuming T has a property called Name, adjust this if needed
                var keywordFilter = Expression.Lambda<Func<T, bool>>(
                    Expression.Call(
                        Expression.Property(Expression.Parameter(typeof(T), "p"), "Name"),
                        "Contains",
                        Type.EmptyTypes,
                        Expression.Constant(pagingParameters.Keyword)
                    ),
                    Expression.Parameter(typeof(T), "p")
                );
                query = query.Where(keywordFilter);
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
