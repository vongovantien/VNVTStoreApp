using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;

using VNVTStore.Application.Suppliers.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.Suppliers.Handlers;

public class SupplierHandlers : BaseHandler<TblSupplier>,
    IRequestHandler<CreateCommand<CreateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<UpdateCommand<UpdateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<DeleteCommand<TblSupplier>, Result>,
    IRequestHandler<DeleteMultipleCommand<TblSupplier>, Result>,
    IRequestHandler<GetPagedQuery<SupplierDto>, Result<PagedResult<SupplierDto>>>, // Add this explicitly
    IRequestHandler<GetAllSuppliersQuery, Result<PagedResult<SupplierDto>>>, // Keep existing
    IRequestHandler<GetByCodeQuery<SupplierDto>, Result<SupplierDto>>
{
    private readonly IRepository<TblProduct> _productRepository;

    public SupplierHandlers(
        IRepository<TblSupplier> repository,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(repository, unitOfWork, mapper, dapperContext)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<SupplierDto>> Handle(CreateCommand<CreateSupplierDto, SupplierDto> request, CancellationToken cancellationToken)
    {
        return await CreateAsync<CreateSupplierDto, SupplierDto>(
            request.Dto, 
            cancellationToken,
            s => {
                s.Code = Guid.NewGuid().ToString("N").Substring(0, 10);
                s.IsActive = true;
            });
    }

    public async Task<Result<SupplierDto>> Handle(UpdateCommand<UpdateSupplierDto, SupplierDto> request, CancellationToken cancellationToken)
    {
        return await UpdateAsync<UpdateSupplierDto, SupplierDto>(
            request.Code, 
            request.Dto, 
            MessageConstants.Supplier,
            cancellationToken);
    }

    public async Task<Result> Handle(DeleteCommand<TblSupplier> request, CancellationToken cancellationToken)
    {
        var productCount = await _productRepository.CountAsync(p => p.SupplierCode == request.Code && p.IsActive == true, cancellationToken);

        if (productCount > 0)
        {
             return Result.Failure(Error.Conflict(MessageConstants.Conflict, 
                 $"Cannot delete supplier because they have {productCount} active products."));
        }

        return await DeleteAsync(request.Code, MessageConstants.Supplier, cancellationToken);
    }

    public async Task<Result> Handle(DeleteMultipleCommand<TblSupplier> request, CancellationToken cancellationToken)
    {
        // Check for suppliers with active products
        var blockedCodesQuery = _productRepository.AsQueryable()
             .Where(p => p.SupplierCode != null && request.Codes.Contains(p.SupplierCode) && p.IsActive == true)
             .Select(p => p.SupplierCode!);

        var checkResult = await CheckBlockingDependenciesAsync(blockedCodesQuery, "products", cancellationToken);
        if (checkResult.IsFailure) return checkResult;

        return await DeleteMultipleAsync(request.Codes, MessageConstants.Supplier, cancellationToken);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

        // Handle IsActive filter if present in query
        string? whereClause = null;
        object? parameters = null;
        if (request.IsActive.HasValue)
        {
             // Dapper WHERE generation needs to be handled or pass active filter
             // BaseHandler GetPagedDapperAsync might support filters if extended or use SQL string builder
             // For now, standard search.
        }

        return await GetPagedDapperAsync<SupplierDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            sortDTO,
            null,
            request.Fields,
            cancellationToken);
    }

    public async Task<Result<SupplierDto>> Handle(GetByCodeQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<SupplierDto>(request.Code, MessageConstants.Supplier, cancellationToken);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetPagedQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        var searchFields = request.Searching ?? new List<SearchDTO>();
        if (!string.IsNullOrEmpty(request.Search))
        {
             searchFields.Add(new SearchDTO { SearchField = "Name", SearchCondition = SearchCondition.Contains, SearchValue = request.Search });
        }
        
        // Use default sort if not provided
        var sort = request.SortDTO ?? new SortDTO { SortBy = "CreatedAt", SortDescending = true };

        return await GetPagedDapperAsync<SupplierDto>(
            request.PageIndex, 
            request.PageSize, 
            searchFields, 
            sort, 
            null, 
            request.Fields, 
            cancellationToken);
    }
}
