using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Common;

public abstract class BaseHandler<TEntity> where TEntity : class
{
    protected readonly IRepository<TEntity> Repository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;

    protected BaseHandler(IRepository<TEntity> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
        Mapper = mapper;
    }

    protected async Task<Result<TResponse>> CreateAsync<TCreateDto, TResponse>(
        TCreateDto dto, 
        CancellationToken cancellationToken,
        Action<TEntity>? beforeSave = null)
    {
        try 
        {
            await UnitOfWork.BeginTransactionAsync(cancellationToken);

            var entity = Mapper.Map<TEntity>(dto);
            
            beforeSave?.Invoke(entity);

            await Repository.AddAsync(entity, cancellationToken);
            await UnitOfWork.CommitAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(Mapper.Map<TResponse>(entity));
        }
        catch (Exception)
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    protected async Task<Result<TResponse>> UpdateAsync<TUpdateDto, TResponse>(
        string code, 
        TUpdateDto dto, 
        string entityName,
        CancellationToken cancellationToken,
        Action<TEntity>? beforeSave = null)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync(cancellationToken);

            var entity = await Repository.GetByCodeAsync(code, cancellationToken);
            if (entity == null)
                return Result.Failure<TResponse>(Error.NotFound(entityName, code));

            Mapper.Map(dto, entity);
            
            beforeSave?.Invoke(entity);

            Repository.Update(entity);
            await UnitOfWork.CommitAsync(cancellationToken);
            await UnitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(Mapper.Map<TResponse>(entity));
        }
        catch (Exception)
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    protected async Task<Result> DeleteAsync(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        bool softDelete = true)
    {
        var entity = await Repository.GetByCodeAsync(code, cancellationToken);
        if (entity == null)
            return Result.Failure(Error.NotFound(entityName, code));

        // Check if active
        var isActiveProp = typeof(TEntity).GetProperty("IsActive");
        if (isActiveProp != null && isActiveProp.PropertyType == typeof(bool))
        {
            var isActive = (bool)isActiveProp.GetValue(entity)!;
            if (isActive)
            {
               return Result.Failure(Error.Conflict($"{entityName} is active. Please deactivate first."));
            }
        }

        if (softDelete)
        {
            if (isActiveProp != null && isActiveProp.PropertyType == typeof(bool))
            {
                isActiveProp.SetValue(entity, false);
                Repository.Update(entity);
            }
            else
            {
                Repository.Delete(entity);
            }
        }
        else
        {
            Repository.Delete(entity);
        }

        await UnitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    protected async Task<Result<PagedResult<TResponse>>> GetPagedAsync<TResponse>(
        int pageIndex, 
        int pageSize, 
        CancellationToken cancellationToken,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        var query = Repository.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        if (includes != null)
            query = includes(query);

        var totalItems = await query.CountAsync(cancellationToken);

        if (orderBy != null)
            query = orderBy(query);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = Mapper.Map<List<TResponse>>(items);
        return Result.Success(new PagedResult<TResponse>(dtos, totalItems, pageIndex, pageSize));
    }

    protected async Task<Result<TResponse>> GetByCodeAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null)
    {
        var query = Repository.AsQueryable();
        
        if (includes != null)
            query = includes(query);

        var entity = await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Code") == code, cancellationToken);
        
        if (entity == null)
            return Result.Failure<TResponse>(Error.NotFound(entityName, code));

        return Result.Success(Mapper.Map<TResponse>(entity));
    }
}
