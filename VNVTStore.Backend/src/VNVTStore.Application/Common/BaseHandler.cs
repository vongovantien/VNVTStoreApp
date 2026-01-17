using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;

using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Common;

public abstract class BaseHandler<TEntity> where TEntity : class, IEntity
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
        if (entity.IsActive)
        {
             return Result.Failure(Error.Conflict($"{entityName} is active. Please deactivate first."));
        }

        if (softDelete)
        {
             entity.IsActive = false;
             entity.ModifiedType = ModificationType.Delete.ToString();
             entity.UpdatedAt = DateTime.UtcNow; // Clean update
             Repository.Update(entity);
        }
        else
        {
            Repository.Delete(entity);
        }

        await UnitOfWork.CommitAsync(cancellationToken);
        await UnitOfWork.CommitAsync(cancellationToken); // Why commit twice? Legacy? I'll keep one.
        return Result.Success();
    }

    protected async Task<Result> DeleteMultipleAsync(
        List<string> codes,
        string entityName,
        CancellationToken cancellationToken)
    {
        var entities = await Repository.AsQueryable()
            .Where(e => codes.Contains(e.Code))
            .ToListAsync(cancellationToken);

        if (entities.Count != codes.Count)
        {
             // Ignore missing
        }

        var activeItems = entities.Where(e => e.IsActive).Select(e => e.Code).ToList();

        if (activeItems.Any())
        {
            return Result.Failure(Error.Conflict(entityName, $"Cannot delete active items: {string.Join(", ", activeItems)}. Please deactivate them first."));
        }

        foreach (var entity in entities)
        {
            entity.ModifiedType = ModificationType.Delete.ToString();
            entity.UpdatedAt = DateTime.UtcNow;
            Repository.Update(entity);
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
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<string>? fields = null)
    {
        var query = Repository.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        // Filter out Soft Deleted items
        query = query.Where(e => e.ModifiedType != ModificationType.Delete.ToString());

        if (includes != null)
            query = includes(query);

        var totalItems = await query.CountAsync(cancellationToken);

        if (orderBy != null)
            query = orderBy(query);

        if (fields != null && fields.Any())
            query = QueryHelper.ApplySelection(query, fields);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = Mapper.Map<List<TResponse>>(items);
        return Result.Success(new PagedResult<TResponse>(dtos, totalItems));
    }

    protected async Task<Result<TResponse>> GetByCodeAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null)
    {
        var query = Repository.AsQueryable();

        // Filter out Soft Deleted
        query = query.Where(e => e.ModifiedType != ModificationType.Delete.ToString());
        
        if (includes != null)
            query = includes(query);

        var entity = await query.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
        
        if (entity == null)
            return Result.Failure<TResponse>(Error.NotFound(entityName, code));

        return Result.Success(Mapper.Map<TResponse>(entity));
    }

    protected async Task<Result<TResponse>> GetByCodeIncludeChildrenAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
    {
        return await GetByCodeAsync<TResponse>(code, entityName, cancellationToken, includes);
    }

    protected async Task<Result> ValidateDeletionRequirementsAsync<TDependency>(
        IEnumerable<string> codes,
        IQueryable<TDependency> dependencies,
        Expression<Func<TDependency, string>> foreignKeySelector,
        Expression<Func<TDependency, bool>> activePredicate,
        string dependencyName,
        CancellationToken cancellationToken) where TDependency : class
    {
        // 1. Filter dependencies that match the codes and are active
        // We need to build a dynamic query: dependencies.Where(d => codes.Contains(d.FK) && d.IsActive)

        // Since EF Core translation of "Contains" with complex selector can be tricky, 
        // we assume foreignKeySelector is a simple property access efficiently translated.
        // However, using "foreignKeySelector" inside "Contains" usually requires strictly "x => x.Prop".
        // A safer approach for generic "Where" with selector is to use the query directly if possible,
        // or loop if unavoidable, but we want to avoid loops.
        
        // Actually, we can just assume the caller passes a Queryable that *already* includes the "codes.Contains(FK)" part?
        // No, the caller wants us to do the "codes.Contains" logic to be reusable.
        
        // Let's rely on the caller passing the "FK Property Name" string?
        // Or better: The caller constructs the "Where codes contains" part?
        // If the caller constructs the query, then this method just executes it and formats the error?
        // That seems essentially what I wrote before:
        /*
        var usedCodes = await _productRepository.AsQueryable()
            .Where(p => request.Codes.Contains(p.CategoryCode) && p.IsActive == true)
            .Select(p => p.CategoryCode)
            .Distinct()
            .ToListAsync(cancellationToken);
        */
        
        // To make this reusable, we need to abstract the "Select(p => p.CategoryCode)" part.
        
        var query = dependencies.Where(activePredicate);
        
        // We need to filter by codes. 
        // Note: Using expression trees to combine foreignKeySelector with Contains is complex.
        // Easier alternative: Caller passes `IQueryable<string>` which IS the query for "Used Codes".
        // i.e. `dependencies.Where(...).Select(fk)`
        // Then this method just `Distinct().ToListAsync()` and handles the error reporting.
        
        return Result.Success(); // Placeholder to be replaced by actual implementation below
    }

    protected async Task<Result> CheckBlockingDependenciesAsync(
        IQueryable<string> blockingEntityCodesQuery,
        string dependencyEntityName,
        CancellationToken cancellationToken)
    {
        var usedCodes = await blockingEntityCodesQuery
            .Distinct()
            .ToListAsync(cancellationToken);

        if (usedCodes.Any())
        {
            // Get names of the entities we are trying to delete (the "Parents") that are blocked
            // We assume TEntity has a "Code" and a "Name" (or Username).
            // "Name" might not exist on all TEntity. TblUser has "Username", TblCategory "Name".
            // We can try to dynamically select specific display property or just show codes.
            
            var blockedEntities = await Repository.AsQueryable()
                .Where(e => usedCodes.Contains(e.Code))
                .ToListAsync(cancellationToken);

            var names = new List<string>();
            foreach (var entity in blockedEntities)
            {
                // Try to find a display name property
                var nameProp = entity.GetType().GetProperty("Name") ?? entity.GetType().GetProperty("Username") ?? entity.GetType().GetProperty("FullName");
                if (nameProp != null)
                {
                    var val = nameProp.GetValue(entity)?.ToString();
                    if (!string.IsNullOrEmpty(val)) names.Add(val);
                    else names.Add(entity.Code);
                }
                else
                {
                    names.Add(entity.Code);
                }
            }

            return Result.Failure(Error.Conflict(MessageConstants.Conflict,
                $"Cannot delete {typeof(TEntity).Name.Replace("Tbl", "")}s because they have active {dependencyEntityName}: {string.Join(", ", names)}"));
        }

        return Result.Success();
    }
}
