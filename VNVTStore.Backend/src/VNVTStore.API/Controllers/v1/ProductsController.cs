using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Constants;

using VNVTStore.Application.Products.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

using VNVTStore.Application.Products.Commands;

namespace VNVTStore.API.Controllers.v1;

using Microsoft.Extensions.Caching.Memory;

public class ProductsController : BaseApiController<TblProduct, ProductDto, CreateProductDto, UpdateProductDto>
{
    private readonly IMemoryCache _cache;

    public ProductsController(IMediator mediator, IMemoryCache cache) : base(mediator)
    {
        _cache = cache;
    }

    [HttpPost("search")]
    public override async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var cacheKey = VNVTStore.Application.Common.Helpers.CacheKeyHelper.GenerateKeyFromRequest("products_search", request);
        
        bool isAdmin = IsAdmin();
        if (!isAdmin && _cache.TryGetValue(cacheKey, out Result<PagedResult<ProductDto>>? cachedResult) && cachedResult != null)
        {
            return HandleResult(cachedResult);
        }

        // Re-implement base search logic to cache result
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;

        if (!IsAdmin())
        {
            request.Searching ??= new List<SearchDTO>();
            if (!request.Searching.Any(s => s.SearchField.Equals("IsActive", StringComparison.OrdinalIgnoreCase)))
            {
                request.Searching.Add(new SearchDTO 
                { 
                    SearchField = "IsActive", 
                    SearchValue = true, 
                    SearchCondition = SearchCondition.Equal 
                });
            }
        }

        var result = await Mediator.Send(CreatePagedQuery(pageIndex, pageSize, request.SortDTO, request.Searching, request.Fields));
        
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2)); // Cache search for 2 mins
        }
        
        return HandleResult(result);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetProductStatsQuery());
        return HandleResult(result);
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrending([FromQuery] int limit = 8)
    {
        var result = await Mediator.Send(new GetTrendingProductsQuery(limit));
        return HandleResult(result);
    }



    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<IActionResult> Get(string code, [FromQuery] bool includeChildren = false)
    {
        string cacheKey = $"product_details_{code}_{includeChildren}";
        
        if (_cache.TryGetValue(cacheKey, out Result<ProductDto>? cachedResult) && cachedResult != null)
        {
            return HandleResult(cachedResult);
        }

        var result = await Mediator.Send(new GetProductByCodeQuery(code, includeChildren));

        if (result.IsSuccess)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1); // Set size if using size limit, good practice

            _cache.Set(cacheKey, result, cacheOptions);
        }

        return HandleResult(result);
    }

    protected override IRequest<Result<ProductDto>> CreateGetByCodeQuery(string code, bool includeChildren)
        => new GetProductByCodeQuery(code, includeChildren);

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public override Task<IActionResult> Create([FromBody] RequestDTO<CreateProductDto> request) => base.Create(request);

    [HttpPost("import")]
    [EnableRateLimiting("ExpensiveLimit")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        // Create a memory stream to copy the file content because the request stream might not be seekable or might close
        // MiniExcel might need a seekable stream or just reads it. 
        // Safest is to copy to MemoryStream if file is small, or pass directly. 
        // Using MemoryStream to be safe and independent of Request stream lifetime if async takes long.
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var result = await Mediator.Send(new ImportProductsCommand(memoryStream));
        return HandleResult(result);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var bytes = VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.ProductImportDto>();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products_template.xlsx");
    }

    [HttpGet("{code}/related")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelated(string code, [FromQuery] int limit = 10)
    {
        string cacheKey = $"product_related_{code}_{limit}";
        if (_cache.TryGetValue(cacheKey, out Result<List<ProductDto>>? cached) && cached != null)
        {
            return HandleResult(cached);
        }

        var result = await Mediator.Send(new GetRelatedProductsQuery(code, limit));
        
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        }
        
        return HandleResult(result);
    }

    [HttpPut("{code}")]
    [Authorize(Roles = "admin,Admin")]
    public override async Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateProductDto> request)
    {
        var result = await base.Update(code, request);
        
        // Invalidate cache if update successful
        if (result is OkObjectResult || result is ObjectResult { StatusCode: 200 })
        {
            _cache.Remove($"product_details_{code}_false");
            _cache.Remove($"product_details_{code}_true");
            // Note: Related products cache might also need invalidation if categories/tags changed, 
            // but since key includes limit, it's hard to guess. 30min expiry is fine.
        }
        return result;
    }

    [HttpDelete("{code}")]
    [Authorize(Roles = "admin,Admin")]
    public override async Task<IActionResult> Delete(string code)
    {
        var result = await base.Delete(code);
        
        if (result is OkObjectResult || result is ObjectResult { StatusCode: 200 })
        {
             _cache.Remove($"product_details_{code}_false");
             _cache.Remove($"product_details_{code}_true");
        }
        return result;
    }

    [HttpGet("{code}/questions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(string code)
    {
        var result = await Mediator.Send(new GetProductQuestionsQuery(code));
        return HandleResult(result);
    }

    [HttpPost("{code}/questions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateQuestion(string code, [FromBody] string question)
    {
        var result = await Mediator.Send(new CreateQuestionCommand(code, question));
        return HandleResult(result);
    }
}
