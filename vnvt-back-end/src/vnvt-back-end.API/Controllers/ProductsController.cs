using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    //[MiddlewareFilter(typeof(RequestLocalizationMiddleware))]
    [ApiController]
    public class ProductsController : BaseController<ProductDto, Product
        >
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
           : base(productService)
        {
            _productService = productService;
        }

        public override async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            var response = await _productService.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        public override async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetPaging([FromQuery] PagingParameters pagingParameters)
        {
            var response = await _productService.GetPagedProductsAsync(pagingParameters);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        public override async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
        {
            var response = await _productService.GetAllProductsAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
