using System.Linq.Expressions;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IProductService : IBaseService<ProductDto, Product>
    {
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters);
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    }
}
