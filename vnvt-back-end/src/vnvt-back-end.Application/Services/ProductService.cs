using AutoMapper;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class ProductService : BaseService<Product, ProductDto>, IProductService
    {
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            var result = await base.GetAllAsync(x => x.Category);
            return result;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters)
        {
            var pagedResult = await base.GetPagedAsync(pagingParameters);
            return pagedResult;
        }
    }
}