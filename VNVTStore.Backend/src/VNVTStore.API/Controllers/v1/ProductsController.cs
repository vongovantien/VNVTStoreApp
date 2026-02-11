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

public class ProductsController : BaseApiController<TblProduct, ProductDto, CreateProductDto, UpdateProductDto>
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetProductStatsQuery());
        return HandleResult(result);
    }



    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override Task<IActionResult> Get(string code, [FromQuery] bool includeChildren = false) => base.Get(code, includeChildren);

    protected override IRequest<Result<ProductDto>> CreateGetByCodeQuery(string code, bool includeChildren)
        => new GetProductByCodeQuery(code, includeChildren);

    [HttpPost]
    [Authorize(Roles = "admin,Admin")]
    public override Task<IActionResult> Create([FromBody] RequestDTO<CreateProductDto> request) => base.Create(request);

    [HttpPost("import")]
    [EnableRateLimiting("ExpensiveLimit")]
    [Authorize(Roles = "admin,Admin")]
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
}
