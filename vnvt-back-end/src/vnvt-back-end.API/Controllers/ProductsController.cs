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
    public class ProductsController : BaseController<ProductDto, Product>
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
        [HttpGet("get-product-filters")]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProductFilter([FromQuery] ProductFilter filter)
        {
            var response = await _productService.GetProductFilters(filter);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        public override async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
        {
            var response = await _productService.GetAllProductsAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            try
            {
                using var stream = file.OpenReadStream();
                await _productService.ImportProductsFromExcelAsync(file);
                return Ok("Products imported successfully");
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                return StatusCode(500, $"An error occurred while importing products: {ex.Message}");
            }
        }

        [HttpGet("sales-report")]
        public async Task<IActionResult> GenerateReport([FromQuery] ReportRequest reportRequest)
        {
            var reportStream = await _productService.GenerateSalesReportAsync(reportRequest);

            var fileName = $"ProductSalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(reportStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("{productId}/frequently-bought-together")]
        public async Task<IActionResult> GetFrequentlyBoughtTogether(int productId)
        {
            var response = await _productService.GetFrequentlyBoughtTogetherAsync(productId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("todays-recommendations")]
        public async Task<IActionResult> GetTodaysRecommendations()
        {
            var response = await _productService.GetTodaysRecommendationsAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
