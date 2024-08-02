using System.Linq.Expressions;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;
using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IProductService : IBaseService<ProductDto>
    {
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters);

    }
}
