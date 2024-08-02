using System.Linq.Expressions;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<PagedResult<T>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
    }
}
