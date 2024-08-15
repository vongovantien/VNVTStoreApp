using Microsoft.AspNetCore.Http;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IProductService : IBaseService<ProductDto, Product>
    {
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters);
        Task<ApiResponse<PagedResult<ProductDto>>> GetProductFilters(ProductFilter pagingParameters);
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
        Task ImportProductsFromExcelAsync(IFormFile file);
        Task<MemoryStream> GenerateSalesReportAsync(ReportRequest reportRequest);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetFrequentlyBoughtTogetherAsync(int productId);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetTodaysRecommendationsAsync();
    }
}
