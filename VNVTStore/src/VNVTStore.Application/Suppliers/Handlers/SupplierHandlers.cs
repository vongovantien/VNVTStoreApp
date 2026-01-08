using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.Suppliers.Commands;
using VNVTStore.Application.Suppliers.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Suppliers.Handlers;

public class SupplierHandlers :
    IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>,
    IRequestHandler<UpdateSupplierCommand, Result<SupplierDto>>,
    IRequestHandler<DeleteSupplierCommand, Result<bool>>,
    IRequestHandler<GetAllSuppliersQuery, Result<PagedResult<SupplierDto>>>,
    IRequestHandler<GetSupplierByCodeQuery, Result<SupplierDto>>
{
    private readonly IRepository<TblSupplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierHandlers(
        IRepository<TblSupplier> supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new TblSupplier
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxCode = request.TaxCode,
            BankAccount = request.BankAccount,
            BankName = request.BankName,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByCodeAsync(request.SupplierCode, cancellationToken);

        if (supplier == null)
            return Result.Failure<SupplierDto>(Error.NotFound("Supplier", request.SupplierCode));

        if (request.Name != null) supplier.Name = request.Name;
        if (request.ContactPerson != null) supplier.ContactPerson = request.ContactPerson;
        if (request.Email != null) supplier.Email = request.Email;
        if (request.Phone != null) supplier.Phone = request.Phone;
        if (request.Address != null) supplier.Address = request.Address;
        if (request.TaxCode != null) supplier.TaxCode = request.TaxCode;
        if (request.BankAccount != null) supplier.BankAccount = request.BankAccount;
        if (request.BankName != null) supplier.BankName = request.BankName;
        if (request.Notes != null) supplier.Notes = request.Notes;
        if (request.IsActive.HasValue) supplier.IsActive = request.IsActive.Value;

        supplier.UpdatedAt = DateTime.UtcNow;
        _supplierRepository.Update(supplier);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<bool>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByCodeAsync(request.SupplierCode, cancellationToken);

        if (supplier == null)
            return Result.Failure<bool>(Error.NotFound("Supplier", request.SupplierCode));

        // Soft delete
        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;
        _supplierRepository.Update(supplier);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = _supplierRepository.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(s => 
                s.Name.Contains(request.Search) ||
                (s.Email != null && s.Email.Contains(request.Search)) ||
                (s.Phone != null && s.Phone.Contains(request.Search)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<SupplierDto>>(items);
        return Result.Success(new PagedResult<SupplierDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierByCodeQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByCodeAsync(request.SupplierCode, cancellationToken);

        if (supplier == null)
            return Result.Failure<SupplierDto>(Error.NotFound("Supplier", request.SupplierCode));

        return Result.Success(_mapper.Map<SupplierDto>(supplier));
    }
}
