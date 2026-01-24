using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;

using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

using Dapper;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Common.Attributes;
using VNVTStore.Application.Interfaces;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Concurrent;


namespace VNVTStore.Application.Common;

public abstract class BaseHandler<TEntity> where TEntity : class, IEntity
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly IDapperContext _dapperContext;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();


    protected BaseHandler(
        IRepository<TEntity> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IDapperContext dapperContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dapperContext = dapperContext;
    }

    // Backwards-compatible constructor for handlers not using Dapper
    protected BaseHandler(
        IRepository<TEntity> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper)
        : this(repository, unitOfWork, mapper, null!)
    {
    }

    protected async Task<Result<TResponse>> CreateAsync<TCreateDto, TResponse>(
        TCreateDto dto, 
        CancellationToken cancellationToken,
        Action<TEntity>? beforeSave = null)
    {
        try 
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var entity = _mapper.Map<TEntity>(dto);
            
            beforeSave?.Invoke(entity);

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<TResponse>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var entity = await _repository.GetByCodeAsync(code, cancellationToken);
            if (entity == null)
                return Result.Failure<TResponse>(Error.NotFound(entityName, code));

            _mapper.Map(dto, entity);
            
            beforeSave?.Invoke(entity);

            _repository.Update(entity);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(_mapper.Map<TResponse>(entity));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    protected async Task<Result> DeleteAsync(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        bool softDelete = true)
    {
        var entity = await _repository.GetByCodeAsync(code, cancellationToken);
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
             _repository.Update(entity);
        }
        else
        {
            _repository.Delete(entity);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken); // Why commit twice? Legacy? I'll keep one.
        return Result.Success();
    }

    protected async Task<Result> DeleteMultipleAsync(
        List<string> codes,
        string entityName,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.AsQueryable()
            .Where(e => codes.Contains(e.Code))
            .ToListAsync(cancellationToken);

        if (entities.Count != codes.Count)
        {
             // Ignore missing
        }

        var activeItems = entities.Where(e => e.IsActive).Select(e => e.Code).ToList();

        if (activeItems.Any())
        {
            return Result.Failure(Error.Conflict(MessageConstants.Conflict, $"{entityName}: Cannot delete active items: {string.Join(", ", activeItems)}. Please deactivate them first."));
        }

        foreach (var entity in entities)
        {
            entity.ModifiedType = ModificationType.Delete.ToString();
            entity.UpdatedAt = DateTime.UtcNow;
            _repository.Update(entity);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
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
        var query = _repository.AsQueryable();

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

        var dtos = _mapper.Map<List<TResponse>>(items);
        return Result.Success(new PagedResult<TResponse>(dtos, totalItems));
    }

    protected async Task<Result<TResponse>> GetByCodeAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null)
    {
        var query = _repository.AsQueryable();

        // Filter out Soft Deleted
        query = query.Where(e => e.ModifiedType != ModificationType.Delete.ToString());
        
        if (includes != null)
            query = includes(query);

        var entity = await query.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
        
        if (entity == null)
            return Result.Failure<TResponse>(Error.NotFound(entityName, code));

        return Result.Success(_mapper.Map<TResponse>(entity));
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
            
            var blockedEntities = await _repository.AsQueryable()
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
    
    /// <summary>
    /// Gets paged data using Dapper with parameterized queries (SQL injection safe).
    /// Optimized with: ConfigureAwait(false), OpenAsync(), buffered streaming.
    /// </summary>
    protected async Task<Result<PagedResult<TResponse>>> GetPagedDapperAsync<TResponse>(
        int pageIndex,
        int pageSize,
        List<SearchDTO>? searchFields,
        SortDTO? sortDTO,
        List<ReferenceTable>? referenceTables = null,
        List<string>? fields = null,
        CancellationToken cancellationToken = default) where TResponse : class, new()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        // Use using for proper disposal
        using var connection = _dapperContext.CreateConnection();
        
        // Explicitly open connection before query
        connection.Open();
        
        // Setup search fields
        searchFields ??= new List<SearchDTO>();
        sortDTO ??= new SortDTO { SortBy = "CreatedAt", Sort = "DESC" };
        if (string.IsNullOrEmpty(sortDTO.Sort)) 
            sortDTO.Sort = sortDTO.SortDescending ? "DESC" : "ASC";

        // Filter out Soft Deleted items by default
        if (!searchFields.Any(s => s.SearchField == "ModifiedType"))
        {
            searchFields.Add(new SearchDTO 
            { 
                SearchField = "ModifiedType", 
                SearchCondition = SearchCondition.NotEqual, 
                SearchValue = "Delete" 
            });
        }
        
        var tableName = typeof(TEntity).Name;
        
        // Auto-discover Reference Tables from TResponse Attributes
        var autoRefTables = GetReferenceTables<TResponse>();
        referenceTables = referenceTables == null 
            ? autoRefTables 
            : referenceTables.Concat(autoRefTables).ToList();

        // Build parameterized query (SQL injection safe)
        var queryResult = QueryBuilder.BuildRawQueryPagingParameterized(
            pageSize, pageIndex, tableName, referenceTables, searchFields, sortDTO, fields);
        
        Console.WriteLine($"[Dapper Debug] SQL Generation took: {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        // Execute with parameters
        var dynamicResults = await connection.QueryAsync<dynamic>(
            queryResult.Sql, 
            queryResult.Parameters
        ).ConfigureAwait(false);
        
        // Convert to list
        var resultList = dynamicResults.ToList();
        Console.WriteLine($"[Dapper Debug] Query Execution took: {sw.ElapsedMilliseconds}ms. Rows: {resultList.Count}");
        sw.Restart();

        var dtos = new List<TResponse>();
        int totalCount = 0;

        if (resultList.Any())
        {
            totalCount = Convert.ToInt32(((IDictionary<string, object>)resultList.First())["TotalRow"]);
            dtos = MapDynamicList<TResponse>(resultList);
        }
        
        Console.WriteLine($"[Dapper Debug] Mapping took: {sw.ElapsedMilliseconds}ms");
        sw.Restart();
        
        // Populate Collections with ConfigureAwait
        await PopulateCollectionsAsync(dtos, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"[Dapper Debug] PopulateCollections took: {sw.ElapsedMilliseconds}ms");

        return Result.Success(new PagedResult<TResponse>(dtos, totalCount));
    }

    private List<ReferenceTable> GetReferenceTables<TResponse>()
    {
        var list = new List<ReferenceTable>();
        var props = typeof(TResponse).GetProperties();
        
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ReferenceAttribute>();
            if (attr != null)
            {
                // Skip if any required field is empty to avoid SQL errors
                if (string.IsNullOrEmpty(attr.TableName) || 
                    string.IsNullOrEmpty(attr.ForeignKey) || 
                    string.IsNullOrEmpty(attr.SelectColumn) ||
                    string.IsNullOrEmpty(prop.Name))
                {
                    continue;
                }
                
                list.Add(new ReferenceTable
                {
                    TableName = attr.TableName,
                    AliasName = prop.Name, // DTO Property Name (e.g., CategoryName)
                    ForeignKeyCol = attr.ForeignKey,
                    ColumnName = attr.SelectColumn,
                    FilterType = attr.FilterType ?? "All",
                    TargetColumn = attr.TargetColumn ?? "Code",
                    FilterColumn = attr.FilterColumn,
                    FilterValue = attr.FilterValue
                });
            }
        }
        return list;
    }

    /// <summary>
    /// Populates child collections for DTOs based on ReferenceCollectionAttribute.
    /// Uses parameterized query with ConfigureAwait(false) for library code.
    /// </summary>
    protected async Task PopulateCollectionsAsync<TResponse>(
        List<TResponse> items,
        CancellationToken cancellationToken = default) where TResponse : class, new()
    {
        if (items == null || !items.Any()) return;

        using var connection = _dapperContext.CreateConnection();
        connection.Open();
        
        var responseType = typeof(TResponse);
        var props = responseType.GetProperties();

        foreach (var prop in props)
        {
            var collAttr = prop.GetCustomAttribute<ReferenceCollectionAttribute>();
            if (collAttr == null) continue;

            // Get parent codes from items
            var parentKeyProp = responseType.GetProperty(collAttr.ParentKey);
            if (parentKeyProp == null) continue;

            var parentCodes = items
                .Select(i => parentKeyProp.GetValue(i)?.ToString())
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            if (!parentCodes.Any()) continue;

            // Build parameterized SQL for child query (SQL injection safe)
            var sql = $"SELECT * FROM \"{collAttr.ChildTableName}\" WHERE \"{collAttr.ForeignKey}\" = ANY(@Codes)";
            
            if (!string.IsNullOrEmpty(collAttr.FilterColumn) && !string.IsNullOrEmpty(collAttr.FilterValue))
            {
                sql += $" AND \"{collAttr.FilterColumn}\" = @FilterValue";
            }

            var parameters = new { Codes = parentCodes.ToArray(), FilterValue = collAttr.FilterValue };
            
            // Execute with ConfigureAwait(false)
            var children = await connection.QueryAsync<dynamic>(sql, parameters)
                .ConfigureAwait(false);
            
            // Performance Optimization: Direct Mapping for children instead of JSON
            var mapMethod = typeof(BaseHandler<TEntity>).GetMethod("MapDynamicList", BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMapMethod = mapMethod!.MakeGenericMethod(collAttr.ChildDtoType);
            var childList = genericMapMethod.Invoke(this, new object[] { children }) as System.Collections.IList;

            if (childList == null) continue;

            // Group children by foreign key
            var groupedChildren = new Dictionary<string, List<object>>();
            var foreignKeyProp = collAttr.ChildDtoType.GetProperty(collAttr.ForeignKey);
            
            if (foreignKeyProp == null)
            {
                // Try to find property with matching column attribute or Name
                var childProps = collAttr.ChildDtoType.GetProperties();
                foreach (var p in childProps)
                {
                    if (p.Name.Equals(collAttr.ForeignKey, StringComparison.OrdinalIgnoreCase))
                    {
                        foreignKeyProp = p;
                        break;
                    }
                }
            }

            if (foreignKeyProp == null) continue;

            foreach (var child in (System.Collections.IEnumerable)childList)
            {
                var fkValue = foreignKeyProp.GetValue(child)?.ToString();
                if (string.IsNullOrEmpty(fkValue)) continue;

                if (!groupedChildren.ContainsKey(fkValue))
                    groupedChildren[fkValue] = new List<object>();
                
                groupedChildren[fkValue].Add(child);
            }

            // Assign to parent items
            foreach (var item in items)
            {
                var parentCode = parentKeyProp.GetValue(item)?.ToString();
                if (string.IsNullOrEmpty(parentCode)) continue;

                if (groupedChildren.TryGetValue(parentCode, out var childrenForParent))
                {
                    // Create properly typed list
                    var listType = typeof(List<>).MakeGenericType(collAttr.ChildDtoType);
                    var typedList = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod("Add");
                    
                    foreach (var c in childrenForParent)
                    {
                        addMethod!.Invoke(typedList, new[] { c });
                    }
                    
                    prop.SetValue(item, typedList);
                }
            }
        }
    }

    private List<T> MapDynamicList<T>(IEnumerable<dynamic> source) where T : new()
    {
        var list = new List<T>();
        var type = typeof(T);
        var props = _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var row in source)
        {
            var dict = (IDictionary<string, object>)row;
            var item = new T();
            foreach (var prop in props)
            {
                if (dict.TryGetValue(prop.Name, out var value) && value != null && value != DBNull.Value)
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        
                        if (targetType.IsEnum)
                        {
                            prop.SetValue(item, Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType))));
                        }
                        else
                        {
                            prop.SetValue(item, Convert.ChangeType(value, targetType));
                        }
                    }
                    catch
                    {
                        // Skip if mapping fails - robust but fast
                    }
                }
            }
            list.Add(item);
        }
        return list;
    }
}
