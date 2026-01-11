using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;

using VNVTStore.Application.Suppliers.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Suppliers.Handlers;

public class SupplierHandlers : BaseHandler<TblSupplier>,
    IRequestHandler<CreateCommand<CreateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<UpdateCommand<UpdateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<DeleteCommand<TblSupplier>, Result>,
    IRequestHandler<GetPagedQuery<SupplierDto>, Result<PagedResult<SupplierDto>>>,
    IRequestHandler<GetByCodeQuery<SupplierDto>, Result<SupplierDto>>
{
    public SupplierHandlers(
        IRepository<TblSupplier> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(repository, unitOfWork, mapper)
    {
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
        return await DeleteAsync(request.Code, MessageConstants.Supplier, cancellationToken);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetPagedQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedAsync<SupplierDto>(
            request.PageIndex, 
            request.PageSize, 
            cancellationToken,
            predicate: s => (string.IsNullOrEmpty(request.Search) || (s.Name.Contains(request.Search) || (s.Email != null && s.Email.Contains(request.Search)) || (s.Phone != null && s.Phone.Contains(request.Search)))),
            orderBy: q => q.OrderByDescending(s => s.CreatedAt));
    }

    public async Task<Result<SupplierDto>> Handle(GetByCodeQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<SupplierDto>(request.Code, MessageConstants.Supplier, cancellationToken);
    }
}
