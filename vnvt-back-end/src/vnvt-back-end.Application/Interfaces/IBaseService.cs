using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IBaseService<TDto, TEntity>
    where TEntity : class
    {
        Task<ApiResponse<IEnumerable<TDto>>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<PagedResult<TDto>>> GetPagedAsync(PagingParameters pagingParameters, Expression<Func<TDto, bool>> filter = null, params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<TDto>> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<TDto>> AddAsync(TDto dto);
        Task<ApiResponse<TDto>> UpdateAsync(TDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

}
