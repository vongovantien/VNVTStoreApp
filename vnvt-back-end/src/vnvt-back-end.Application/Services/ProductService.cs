using AutoMapper;
using System.Linq.Expressions;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;
using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.Services
{
    public class ProductService : BaseService<Product, ProductDto>, IProductService
    {
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            var items = await _unitOfWork.Products.GetAllProductsAsync();
            var result = _mapper.Map<IEnumerable<ProductDto>>(items);
            return ApiResponseBuilder.Success(result);
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters)
        {
            var pagedResult = await _unitOfWork.Products.GetPagedProductsAsync(pagingParameters);
            var items = _mapper.Map<IEnumerable<ProductDto>>(pagedResult.Items);
            var result = new PagedResult<ProductDto>(items, pagedResult.TotalItems, pagedResult.PageNumber, pagedResult.PageSize);
            return ApiResponseBuilder.Success(result);
        }
    }
}