using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Products.Handlers;

public class ProductHandlers : BaseHandler<TblProduct>,
    IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>,
    IRequestHandler<GetPagedQuery<ProductDto>, Result<PagedResult<ProductDto>>>,
    IRequestHandler<GetByCodeQuery<ProductDto>, Result<ProductDto>>,
    IRequestHandler<CreateCommand<CreateProductDto, ProductDto>, Result<ProductDto>>,
    IRequestHandler<UpdateCommand<UpdateProductDto, ProductDto>, Result<ProductDto>>,
    IRequestHandler<DeleteCommand<TblProduct>, Result>
{
    public ProductHandlers(IRepository<TblProduct> repository, IUnitOfWork unitOfWork, IMapper mapper) 
        : base(repository, unitOfWork, mapper)
    {
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetPagedQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        string? categoryCode = null;
        if (request is GetProductsQuery productsQuery)
        {
            categoryCode = productsQuery.CategoryCode;
        }

        return await GetPagedAsync<ProductDto>(
            request.PageIndex,
            request.PageSize,
            cancellationToken,
            predicate: p => (p.IsActive == true) && 
                           (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search) || (p.Description != null && p.Description.Contains(request.Search))) &&
                           (string.IsNullOrWhiteSpace(categoryCode) || p.CategoryCode == categoryCode),
            includes: q => QueryHelper.ApplyFilters(
                q.Include(p => p.CategoryCodeNavigation).Include(p => p.TblProductImages), 
                request.Filters),
            orderBy: q => {
                if (request.SortDTO != null && !string.IsNullOrWhiteSpace(request.SortDTO.SortBy))
                {
                    return request.SortDTO.SortBy.ToLower() switch
                    {
                        "price" => request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price),
                        "name" => request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
                        "createdat" => request.SortDTO.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                        _ => q.OrderByDescending(p => p.CreatedAt)
                    };
                }
                return q.OrderByDescending(p => p.CreatedAt);
            });
    }

    // Explicit handler for GetProductsQuery - delegates to the generic handler
    public Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return Handle((GetPagedQuery<ProductDto>)request, cancellationToken);
    }

    public async Task<Result<ProductDto>> Handle(GetByCodeQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<ProductDto>(
            request.Code, 
            MessageConstants.Product, 
            cancellationToken,
            includes: q => q.Include(p => p.CategoryCodeNavigation).Include(p => p.TblProductImages));
    }

    public async Task<Result<ProductDto>> Handle(CreateCommand<CreateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Check SKU uniqueness
        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var existingSku = await Repository.FindAsync(p => p.Sku == dto.Sku, cancellationToken);
            if (existingSku != null)
                return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", dto.Sku));
        }

        return await CreateAsync<CreateProductDto, ProductDto>(
            dto, 
            cancellationToken,
            p => {
                if (string.IsNullOrWhiteSpace(p.Sku))
                {
                    p.Sku = $"SKU{DateTime.Now.Ticks.ToString().Substring(10)}";
                }
                
                var random = new Random();
                p.Code = $"P{random.Next(100000, 999999)}";
                p.IsActive = true;
            });
    }

    public async Task<Result<ProductDto>> Handle(UpdateCommand<UpdateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        var product = await Repository.GetByCodeAsync(request.Code, cancellationToken);
        if (product == null)
            return Result.Failure<ProductDto>(Error.NotFound(MessageConstants.Product, request.Code));

        // Check SKU uniqueness if changed
        if (!string.IsNullOrWhiteSpace(request.Dto.Sku) && request.Dto.Sku != product.Sku)
        {
            var existingSku = await Repository.FindAsync(p => p.Sku == request.Dto.Sku && p.Code != request.Code, cancellationToken);
            if (existingSku != null)
                return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", request.Dto.Sku));
        }

        return await UpdateAsync<UpdateProductDto, ProductDto>(
            request.Code, 
            request.Dto, 
            MessageConstants.Product, 
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblProduct> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Product, cancellationToken);
    }
}
