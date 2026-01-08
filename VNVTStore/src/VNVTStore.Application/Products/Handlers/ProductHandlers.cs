using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Products.Queries;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure;

namespace VNVTStore.Application.Products.Handlers;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IRepository<TblProduct> _repository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IRepository<TblProduct> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.AsQueryable().Where(p => p.IsActive == true);

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) || 
                                     (p.Description != null && p.Description.Contains(request.Search)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (request.SortDTO != null && !string.IsNullOrWhiteSpace(request.SortDTO.SortBy))
        {
            query = request.SortDTO.SortBy.ToLower() switch
            {
                "price" => request.SortDTO.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "name" => request.SortDTO.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "createdat" => request.SortDTO.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var items = await query
            .Include(p => p.CategoryCodeNavigation)
            .Include(p => p.TblProductImages)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(items);
        return Result.Success(new PagedResult<ProductDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }
}

public class GetProductByCodeQueryHandler : IRequestHandler<GetProductByCodeQuery, Result<ProductDto>>
{
    private readonly IRepository<TblProduct> _repository;
    private readonly IMapper _mapper;

    public GetProductByCodeQueryHandler(IRepository<TblProduct> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository
            .AsQueryable()
            .Where(p => p.Code == request.Code)
            .Include(p => p.CategoryCodeNavigation)
            .Include(p => p.TblProductImages)
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Code));

        return Result.Success(_mapper.Map<ProductDto>(product));
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<TblProduct> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IRepository<TblProduct> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Check SKU uniqueness if provided
        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var existingSku = await _repository.FindAsync(p => p.Sku == dto.Sku, cancellationToken);
            if (existingSku != null)
                return Result.Failure<ProductDto>(Error.Conflict($"SKU '{dto.Sku}' already exists"));
        }
        else
        {
            // Auto-generate SKU if invalid
            dto.Sku = $"SKU{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        // Map DTO to Entity
        var product = _mapper.Map<TblProduct>(dto);
        
        // Auto-generate Code if not provided
        // Use Random to ensure it fits in 10 chars (P + 6 digits = 7 chars)
        // This avoids any Ticks length ambiguity
        var random = new Random();
        product.Code = $"P{random.Next(100000, 999999)}"; 
        
        product.IsActive = true;
        // product.CreatedAt = DateTime.UtcNow; // Removed to let DB handle default and avoid UTC error

        try 
        {
            await _repository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR CREATING PRODUCT: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                 Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
            }
            throw; // Re-throw to let controller handle or valid middleware
        }

        return Result.Success(_mapper.Map<ProductDto>(product));
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<TblProduct> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IRepository<TblProduct> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        
        if (product == null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Code));

        var dto = request.Dto;

        // Check SKU uniqueness if changed
        if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != product.Sku)
        {
            var existingSku = await _repository.FindAsync(p => p.Sku == dto.Sku && p.Code != request.Code, cancellationToken);
            if (existingSku != null)
                return Result.Failure<ProductDto>(Error.Conflict($"SKU '{dto.Sku}' already exists"));
        }

        // Update fields từ DTO (chỉ update nếu không null)
        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.CostPrice.HasValue) product.CostPrice = dto.CostPrice;
        if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity;
        if (dto.CategoryCode != null) product.CategoryCode = dto.CategoryCode;
        if (dto.Sku != null) product.Sku = dto.Sku;
        if (dto.Weight.HasValue) product.Weight = dto.Weight;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive;

        _repository.Update(product);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<ProductDto>(product));
    }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IRepository<TblProduct> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IRepository<TblProduct> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByCodeAsync(request.Code, cancellationToken);

        if (product == null)
            return Result.Failure(Error.NotFound("Product", request.Code));

        // Soft delete
        product.IsActive = false;
        _repository.Update(product);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
