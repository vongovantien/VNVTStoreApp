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
            var test = await query.ToListAsync();
            return test;
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
            IQueryable<T> query = _context.Set<T>();

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
                // Assuming T has a property called "Name"
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
