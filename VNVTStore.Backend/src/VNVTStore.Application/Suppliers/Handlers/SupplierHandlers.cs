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
    IRequestHandler<GetPagedQuery<SupplierDto>, Result<PagedResult<SupplierDto>>>,
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
             .Where(p => request.Codes.Contains(p.SupplierCode) && p.IsActive == true)
             .Select(p => p.SupplierCode!);

        var checkResult = await CheckBlockingDependenciesAsync(blockedCodesQuery, "products", cancellationToken);
        if (checkResult.IsFailure) return checkResult;

        return await DeleteMultipleAsync(request.Codes, MessageConstants.Supplier, cancellationToken);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetPagedQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        var sortDTO = request.SortDTO ?? new SortDTO { SortBy = request.SortField ?? "CreatedAt", SortDescending = request.SortDescending };

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
}
