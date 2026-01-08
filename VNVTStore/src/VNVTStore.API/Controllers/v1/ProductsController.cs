using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm với phân trang và filter
    /// </summary>
    [HttpPost("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? 1;
        var pageSize = request.PageSize ?? 10;
        
        // Extract search from Searching
        string? search = request.Searching?.FirstOrDefault(s => 
            s.Field?.ToLower() == "name" || s.Field?.ToLower() == "search")?.Value;

        var query = new GetProductsQuery(pageIndex, pageSize, search, request.SortDTO);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result.Value!, "Products retrieved successfully"));
    }

    /// <summary>
    /// Lấy chi tiết sản phẩm theo Code
    /// </summary>
    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(string code)
    {
        var result = await _mediator.Send(new GetProductByCodeQuery(code));

        if (result.IsFailure)
            return NotFound(ApiResponse<string>.Fail(result.Error!.Message, 404));

        return Ok(ApiResponse<ProductDto>.Ok(result.Value!, "Product retrieved successfully"));
    }

    /// <summary>
    /// Tạo sản phẩm mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] RequestDTO<CreateProductDto> request)
    {
        if (request.PostObject == null)
            return BadRequest(ApiResponse<string>.Fail("PostObject is required"));

        var command = new CreateProductCommand(request.PostObject);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error!.Message));

        return CreatedAtAction(
            nameof(GetProduct),
            new { code = result.Value!.Code },
            ApiResponse<ProductDto>.Ok(result.Value!, "Product created successfully"));
    }

    /// <summary>
    /// Cập nhật sản phẩm (Admin only)
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(string code, [FromBody] RequestDTO<UpdateProductDto> request)
    {
        if (request.PostObject == null)
            return BadRequest(ApiResponse<string>.Fail("PostObject is required"));

        var command = new UpdateProductCommand(code, request.PostObject);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return NotFound(ApiResponse<string>.Fail(result.Error!.Message, 404));

        return Ok(ApiResponse<ProductDto>.Ok(result.Value!, "Product updated successfully"));
    }

    /// <summary>
    /// Xóa sản phẩm (Admin only) - Soft delete
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(string code)
    {
        var result = await _mediator.Send(new DeleteProductCommand(code));

        if (result.IsFailure)
            return NotFound(ApiResponse<string>.Fail(result.Error!.Message, 404));

        return NoContent();
    }
}
