using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    //[MiddlewareFilter(typeof(RequestLocalizationMiddleware))]
    [ApiController]
    public class ProductsController : BaseController<ProductDto>
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
           : base(productService)
        {
            _productService = productService;
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            var response = await _productService.GetAllProductsAsync();
            return Ok(response);
        }
    }
}
