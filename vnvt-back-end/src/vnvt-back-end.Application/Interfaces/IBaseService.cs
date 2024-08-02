using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IBaseService<TDto>
    {
        Task<ApiResponse<IEnumerable<TDto>>> GetAllAsync();
        Task<ApiResponse<PagedResult<TDto>>> GetPagingAsync(PagingParameters pagingParameters);
        Task<ApiResponse<TDto>> GetByIdAsync(int id);
        Task<ApiResponse<TDto>> AddAsync(TDto dto);
        Task<ApiResponse<TDto>> UpdateAsync(TDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
