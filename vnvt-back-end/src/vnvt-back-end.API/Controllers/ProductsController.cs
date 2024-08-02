using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Services;
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

        [HttpGet]
        [Route("GetAllProducts")]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAllProductsAsync()
        {
            var response = await _productService.GetAllProductsAsync();
            return StatusCode(response.StatusCode, response);
        }

    }
}
