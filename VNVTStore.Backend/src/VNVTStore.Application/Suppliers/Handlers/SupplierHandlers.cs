using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace VNVTStore.Application.Suppliers.Handlers;

public class SupplierHandlers : BaseHandler<TblSupplier>,
    IRequestHandler<CreateCommand<CreateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<UpdateCommand<UpdateSupplierDto, SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<DeleteCommand<TblSupplier>, Result>,
    IRequestHandler<GetByCodeQuery<SupplierDto>, Result<SupplierDto>>,
    IRequestHandler<GetPagedQuery<SupplierDto>, Result<PagedResult<SupplierDto>>>,
    IRequestHandler<DeleteMultipleCommand<TblSupplier>, Result>,
    IRequestHandler<GetStatsQuery<TblSupplier>, Result<EntityStatsDto>>
{
    public SupplierHandlers(
        IRepository<TblSupplier> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<SupplierDto>> Handle(CreateCommand<CreateSupplierDto, SupplierDto> request, CancellationToken cancellationToken)
    {
         await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate Unique TaxCode/Email
            if (!string.IsNullOrEmpty(request.Dto.TaxCode))
            {
                var exists = await _repository.AsQueryable()
                    .AnyAsync(s => s.TaxCode == request.Dto.TaxCode && s.ModifiedType != "DELETE", cancellationToken);
                if (exists) return Result.Failure<SupplierDto>(Error.Conflict("TaxCode", "Supplier with this Tax Code already exists."));
            }
            
            var entity = _mapper.Map<TblSupplier>(request.Dto);
            
            entity.Code = "SUP" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            entity.ModifiedType = "ADD";

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<SupplierDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<SupplierDto>> Handle(UpdateCommand<UpdateSupplierDto, SupplierDto> request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (entity == null) return Result.Failure<SupplierDto>(Error.NotFound("Supplier", request.Code));

             // Validate Unique TaxCode if changed
            if (!string.IsNullOrEmpty(request.Dto.TaxCode) && request.Dto.TaxCode != entity.TaxCode)
            {
                var exists = await _repository.AsQueryable()
                    .AnyAsync(s => s.TaxCode == request.Dto.TaxCode && s.ModifiedType != "DELETE" && s.Code != request.Code, cancellationToken);
                 if (exists) return Result.Failure<SupplierDto>(Error.Conflict("TaxCode", "Supplier with this Tax Code already exists."));
            }

            _mapper.Map(request.Dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.ModifiedType = "UPDATE";

            _repository.Update(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<SupplierDto>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> Handle(DeleteCommand<TblSupplier> request, CancellationToken cancellationToken)
    {
         // Check Products dependency
         using var connection = _dapperContext.CreateConnection();
         var hasProducts = await SqlMapper.ExecuteScalarAsync<bool>(
            connection,
            "SELECT EXISTS(SELECT 1 FROM \"TblProduct\" WHERE \"SupplierCode\" = @Code AND \"ModifiedType\" != 'DELETE')", 
            new { Code = request.Code });

        if (hasProducts)
        {
            return Result.Failure(Error.Conflict(MessageConstants.Conflict, "Cannot delete Supplier because it has associated Products."));
        }

        return await DeleteAsync(request.Code, "Supplier", cancellationToken);
    }

    public async Task<Result<SupplierDto>> Handle(GetByCodeQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        return await GetByCodeAsync<SupplierDto>(request.Code, "Supplier", cancellationToken);
    }

    public async Task<Result<PagedResult<SupplierDto>>> Handle(GetPagedQuery<SupplierDto> request, CancellationToken cancellationToken)
    {
        return await GetPagedDapperAsync<SupplierDto>(
            request.PageIndex,
            request.PageSize,
            request.Searching,
            request.SortDTO,
            null,
            request.Fields,
            cancellationToken);
    }
    
    public async Task<Result> Handle(DeleteMultipleCommand<TblSupplier> request, CancellationToken cancellationToken)
    {
        return await DeleteMultipleAsync(request.Codes, "Supplier", cancellationToken);
    }

    public async Task<Result<EntityStatsDto>> Handle(GetStatsQuery<TblSupplier> request, CancellationToken cancellationToken)
    {
         var count = await _repository.AsQueryable().CountAsync(cancellationToken);
         // Fixed: TotalCount -> Total, ActiveCount -> Active
         return Result.Success(new EntityStatsDto { Total = count, Active = count });
    }
}
