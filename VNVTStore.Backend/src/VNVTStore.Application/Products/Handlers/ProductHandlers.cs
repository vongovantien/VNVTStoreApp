using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
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
            predicate: p => (p.IsActive == true) && (p.ModifiedType != ModificationType.Delete.ToString()) &&
                           (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search) || p.Code.Contains(request.Search) || (p.Sku != null && p.Sku.Contains(request.Search))) &&
                           (string.IsNullOrWhiteSpace(categoryCode) || p.CategoryCode == categoryCode),
            includes: q => QueryHelper.ApplyFilters(
                q.Include(p => p.CategoryCodeNavigation).Include(p => p.TblProductImages), 
                request.Filters),
            orderBy: q => {
                if (request.SortDTO != null && !string.IsNullOrWhiteSpace(request.SortDTO.SortBy))
                {
                    var sortBy = request.SortDTO.SortBy.ToLower();
                    if (sortBy == nameof(TblProduct.Price).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Price) : q.OrderBy(p => p.Price);
                    if (sortBy == nameof(TblProduct.Name).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name);
                    if (sortBy == nameof(TblProduct.CreatedAt).ToLower())
                        return request.SortDTO.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt);
                    
                    return q.OrderByDescending(p => p.CreatedAt);
                }
                return q.OrderByDescending(p => p.CreatedAt);
            },
            fields: request.Fields);
    }

    // Explicit handler for GetProductsQuery - delegates to the generic handler
    public Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return Handle((GetPagedQuery<ProductDto>)request, cancellationToken);
    }

    public async Task<Result<ProductDto>> Handle(GetByCodeQuery<ProductDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeIncludeChildrenAsync<ProductDto>(
            request.Code, 
            MessageConstants.Product, 
            cancellationToken,
            includes: q => q.Include(p => p.CategoryCodeNavigation).Include(p => p.TblProductImages));
    }

    public async Task<Result<ProductDto>> Handle(CreateCommand<CreateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;

            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var existingSku = await Repository.FindAsync(p => p.Sku == dto.Sku && p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
                if (existingSku != null)
                {
                    await UnitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", dto.Sku));
                }
            }

            var sku = string.IsNullOrWhiteSpace(dto.Sku) 
                ? $"SKU{DateTime.Now.Ticks.ToString().Substring(10)}" 
                : dto.Sku;

            var product = TblProduct.Create(dto.Name, dto.Price, dto.StockQuantity ?? 0, dto.CategoryCode, sku, dto.CostPrice, 
                dto.Weight, dto.SupplierCode, dto.Color, dto.Power, dto.Voltage, dto.Material, dto.Size);
            
            if (dto.Images != null && dto.Images.Any())
            {
                int sortOrder = 1;
                var newImages = dto.Images.Select(imgUrl => new TblProductImage
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = product.Code,
                    ImageUrl = imgUrl,
                    IsPrimary = sortOrder == 1,
                    SortOrder = sortOrder++
                }).ToList();

                if (product.TblProductImages is List<TblProductImage> imageList)
                {
                    imageList.AddRange(newImages);
                }
                else
                {
                    foreach (var img in newImages) product.TblProductImages.Add(img);
                }
            }

            // Ensure other fields are set if needed via new methods or just accept defaults for now
            // product.SetAttributes(dto.Color, dto.Size...); // If I added this

            await Repository.AddAsync(product, cancellationToken);
            await UnitOfWork.CommitAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(Mapper.Map<ProductDto>(product));
        }
        catch (Exception)
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<ProductDto>> Handle(UpdateCommand<UpdateProductDto, ProductDto> request, CancellationToken cancellationToken)
    {
        await UnitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await Repository.AsQueryable()
                .Include(x => x.TblProductImages)
                .FirstOrDefaultAsync(p => p.Code == request.Code && p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
                
            if (product == null)
            {
                await UnitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ProductDto>(Error.NotFound(MessageConstants.Product, request.Code));
            }

            if (!string.IsNullOrWhiteSpace(request.Dto.Sku) && request.Dto.Sku != product.Sku)
            {
                var existingSku = await Repository.FindAsync(p => p.Sku == request.Dto.Sku && p.Code != request.Code && p.ModifiedType != ModificationType.Delete.ToString(), cancellationToken);
                if (existingSku != null)
                {
                    await UnitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<ProductDto>(Error.Conflict(MessageConstants.AlreadyExists, "SKU", request.Dto.Sku));
                }
            }

            product.UpdateInfo(request.Dto.Name ?? product.Name, request.Dto.Price ?? product.Price, request.Dto.Description ?? product.Description, 
                request.Dto.CategoryCode ?? product.CategoryCode, request.Dto.CostPrice ?? product.CostPrice, request.Dto.StockQuantity ?? product.StockQuantity,
                request.Dto.Weight ?? product.Weight, request.Dto.SupplierCode ?? product.SupplierCode, request.Dto.Color ?? product.Color, 
                request.Dto.Power ?? product.Power, request.Dto.Voltage ?? product.Voltage, request.Dto.Material ?? product.Material, 
                request.Dto.Size ?? product.Size, request.Dto.Sku ?? product.Sku);

            if (request.Dto.Images != null && request.Dto.Images.Any())
            {
                 // Remove existing
                product.TblProductImages.Clear();

                // Add new
                int sortOrder = 1;
                var newImages = request.Dto.Images.Select(imgUrl => new TblProductImage
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ProductCode = product.Code,
                    ImageUrl = imgUrl,
                    IsPrimary = sortOrder == 1, 
                    SortOrder = sortOrder++
                }).ToList();

                if (product.TblProductImages is List<TblProductImage> imageList)
                {
                    imageList.AddRange(newImages);
                }
                else
                {
                    foreach (var img in newImages) product.TblProductImages.Add(img);
                }
            }
            // If Images is empty list, it means remove all? Or if null, do nothing?
            // If user deletes all images, frontend sends empty array.
            else if (request.Dto.Images != null && !request.Dto.Images.Any())
            {
                 product.TblProductImages.Clear();
            }
            
            Repository.Update(product);
            await UnitOfWork.CommitAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(Mapper.Map<ProductDto>(product));
        }
        catch (Exception)
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblProduct> request, CancellationToken cancellationToken)
    {
        return await DeleteAsync(request.Code, MessageConstants.Product, cancellationToken);
    }
}
