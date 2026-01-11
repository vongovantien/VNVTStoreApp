using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Constants;

using VNVTStore.Application.Products.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class ProductsController : BaseApiController<ProductDto, CreateProductDto, UpdateProductDto>
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public override async Task<IActionResult> Search([FromBody] RequestDTO request)
    {
        var pageIndex = request.PageIndex ?? AppConstants.Paging.DefaultPageNumber;
        var pageSize = request.PageSize ?? AppConstants.Paging.DefaultPageSize;

        string? search = request.Searching?.FirstOrDefault(s =>
            s.Field?.ToLower() == "name" || s.Field?.ToLower() == "search" || s.Field?.ToLower() == "code")?.Value;
        
        string? categoryCode = request.Searching?.FirstOrDefault(s => s.Field?.ToLower() == "category")?.Value;

        var filters = request.Searching?.Where(s => 
            !new[] { "name", "search", "code", "category" }.Contains(s.Field?.ToLower())
        ).ToList();

        var result = await Mediator.Send(new GetProductsQuery(pageIndex, pageSize, search, request.SortDTO, categoryCode, filters));
        return HandleResult(result);
    }

    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override Task<IActionResult> Get(string code) => base.Get(code);

    [HttpPost]
    [Authorize(Roles = "admin")]
    public override Task<IActionResult> Create([FromBody] RequestDTO<CreateProductDto> request) => base.Create(request);

    [HttpPut("{code}")]
    [Authorize(Roles = "admin")]
    public override Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateProductDto> request) => base.Update(code, request);

    [HttpDelete("{code}")]
    [Authorize(Roles = "admin")]
    public override Task<IActionResult> Delete(string code) => base.Delete(code);

    protected override IRequest<Result<PagedResult<ProductDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters)
        => new GetPagedQuery<ProductDto>(pageIndex, pageSize, search, sort, filters);

    protected override IRequest<Result<ProductDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<ProductDto>(code);

    protected override IRequest<Result<ProductDto>> CreateCreateCommand(CreateProductDto dto)
        => new CreateCommand<CreateProductDto, ProductDto>(dto);

    protected override IRequest<Result<ProductDto>> CreateUpdateCommand(string code, UpdateProductDto dto)
        => new UpdateCommand<UpdateProductDto, ProductDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblProduct>(code);
}
