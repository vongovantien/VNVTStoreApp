using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Categories.Queries;

using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

using Microsoft.Extensions.Caching.Memory;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Constants;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : BaseApiController<TblCategory, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
{
    private readonly IMemoryCache _cache;

    public CategoriesController(IMediator mediator, IMemoryCache cache) : base(mediator)
    {
        _cache = cache;
    }

    [HttpPost("search")]
    public override async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var cacheKey = CacheKeyHelper.GenerateKeyFromRequest("categories_search", request);
        
        if (_cache.TryGetValue(cacheKey, out Result<PagedResult<CategoryDto>>? cachedResult) && cachedResult != null)
        {
            return HandleResult(cachedResult);
        }

        var result = await base.Search(request); // This calls base.Search which calls Mediator

        // Base.Search returns IActionResult, so we can't easily intercept the Result<T>. 
        // Wait, base.Search calls HandleResult.
        // I need to copy the logic of base.Search but intercept the result.
        // OR change base.Search to return the Result? No, it returns IActionResult.
        // I have to reimplement Search logic here to cache the result.
        
        return await CachedSearch(request, cacheKey);
    }
    
    private async Task<IActionResult> CachedSearch(RequestDTO request, string cacheKey)
    {
         var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
         var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;

         // Logic from BaseApiController
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
             _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5)); // Cache 5 mins
         }
         
         return HandleResult(result);
    }

    [HttpGet("{code}")]
    public override async Task<IActionResult> Get(string code, [FromQuery] bool includeChildren = false)
    {
        string cacheKey = $"category_details_{code}_{includeChildren}";
        
        if (_cache.TryGetValue(cacheKey, out Result<CategoryDto>? cachedResult) && cachedResult != null)
        {
            return HandleResult(cachedResult);
        }

        // We can't call base.Get because it returns IActionResult. 
        // Reimplement:
        var result = await Mediator.Send(CreateGetByCodeQuery(code, includeChildren));

        if (result.IsSuccess)
        {
             _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
        }
        
        return HandleResult(result);
    }

    [HttpPut("{code}")]
    public override async Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateCategoryDto> request)
    {
        var result = await base.Update(code, request);
        // We can't easily check if success from IActionResult unless we cast.
        // But invalidating blindly is safe.
        _cache.Remove($"category_details_{code}_true");
        _cache.Remove($"category_details_{code}_false");
        
        // Search cache is hard to invalidate specific keys. Let expire.
        return result;
    }
    
    [HttpDelete("{code}")]
    public override async Task<IActionResult> Delete(string code)
    {
        var result = await base.Delete(code);
        _cache.Remove($"category_details_{code}_true");
        _cache.Remove($"category_details_{code}_false");
        return result;
    }

    [HttpPost("delete-multiple")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public override async Task<IActionResult> DeleteMultiple([FromBody] List<string> codes)
    {
        var result = await Mediator.Send(new DeleteMultipleCommand<TblCategory>(codes));
        
        if (result.IsSuccess)
        {
            foreach(var code in codes)
            {
                _cache.Remove($"category_details_{code}_true");
                _cache.Remove($"category_details_{code}_false");
            }
        }
        return HandleDelete(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        // Cache stats too?
        var cacheKey = "category_stats";
        if (_cache.TryGetValue(cacheKey, out Result<CategoryStatsDto>? cached) && cached != null)
            return HandleResult(cached);

        var result = await Mediator.Send(new GetCategoryStatsQuery());
        if (result.IsSuccess) _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        
        return HandleResult(result);
    }

    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var result = await Mediator.Send(new VNVTStore.Application.Categories.Commands.ImportCategoriesCommand(memoryStream));
        return HandleResult(result);
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var bytes = VNVTStore.Application.Common.Helpers.ExcelExportHelper.GenerateTemplate<VNVTStore.Application.DTOs.Import.CategoryImportDto>();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "categories_template.xlsx");
    }
}
