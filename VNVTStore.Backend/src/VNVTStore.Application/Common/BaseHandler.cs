using Serilog;
using AutoMapper;
#pragma warning disable CS8602 // Dereference of a possibly null reference
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Interfaces;

using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using System.Data;

using Dapper;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Common.Attributes;
using VNVTStore.Application.Interfaces;
using System.Reflection;
using System.Collections;
using System.Collections.Concurrent;


namespace VNVTStore.Application.Common;

public abstract class BaseHandler<TEntity>
    // Removed interfaces to prevent ambiguity and generic type mismatch
    where TEntity : class, IEntity
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly IDapperContext _dapperContext;
    protected readonly IAuditLogService? _auditLogService;
    protected readonly string _entityName;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();


    protected BaseHandler(
        IRepository<TEntity> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IDapperContext dapperContext,
        IAuditLogService? auditLogService = null)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dapperContext = dapperContext;
        _auditLogService = auditLogService;
        _entityName = typeof(TEntity).Name.Replace("Tbl", "");
    }

    // Backwards-compatible constructor for handlers not using Dapper
    protected BaseHandler(
        IRepository<TEntity> repository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper)
        : this(repository, unitOfWork, mapper, null!)
    {
    }

    protected List<string>? FilterAndValidateFields<TResponse>(List<string>? fields, List<ReferenceTable>? referenceTables)
    {
        if (fields == null || !fields.Any()) return null;

        var collectionProps = typeof(TResponse).GetProperties()
            .Where(p => Attribute.IsDefined(p, typeof(ReferenceCollectionAttribute)))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sqlFields = fields.Where(f => !collectionProps.Contains(f)).ToList();
        
        var entityType = typeof(TEntity);
        var entityProps = entityType.GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var validFields = new List<string>();
        var refTableAliases = referenceTables?.Select(r => r.AliasName).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
        
        foreach (var fieldName in sqlFields)
        {
            if (refTableAliases.Contains(fieldName))
            {
                validFields.Add(fieldName);
                continue;
            }

            if (entityProps.TryGetValue(fieldName, out var prop))
            {
                // Skip if NotMapped
                if (Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
                {
                    continue;
                }

                var columnAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
                if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.Name))
                {
                    validFields.Add(columnAttr.Name);
                }
                else
                {
                    validFields.Add(fieldName);
                }
                continue;
            }
        }
        
        if (validFields.Count == 0 && fields.Count > 0)
        {
             validFields.Add("Code");
        }
        else if (fields != null && fields.Any() && !validFields.Contains("Code", StringComparer.OrdinalIgnoreCase) && !validFields.Contains("\"Code\"", StringComparer.OrdinalIgnoreCase))
        {
            // Always include Code if fields are specified, as it's needed for child population
            validFields.Add("Code");
        }
        
        return validFields;
    }

    public virtual async Task<Result<PagedResult<TResponse>>> Handle<TResponse>(GetPagedQuery<TResponse> request, CancellationToken cancellationToken) where TResponse : class, new()
    {
            return await GetPagedDapperAsync<TResponse>(request.PageIndex, request.PageSize, request.Searching, request.SortDTO, null, request.Fields, cancellationToken);
    }
    
    public virtual async Task<Result> Handle(DeleteMultipleCommand<TEntity> request, CancellationToken cancellationToken)
    {
        return await DeleteMultipleAsync(request.Codes, _entityName, cancellationToken);
    }

    public virtual async Task<Result<TResponse>> Handle<TResponse>(GetByCodeQuery<TResponse> request, CancellationToken cancellationToken) where TResponse : class, new()
    {
        return await GetByCodeAsync<TResponse>(request.Code, _entityName, cancellationToken);
    }

    protected virtual async Task<Result<byte[]>> ExportAllAsync<TResponse>(CancellationToken cancellationToken) where TResponse : class, new()
    {
        // Fetch all items (up to 10k for safety)
        var result = await GetPagedDapperAsync<TResponse>(1, 10000, null, null, null, null, cancellationToken);
        if (result.IsFailure) return Result.Failure<byte[]>(result.Error!);

        var bytes = ExcelExportHelper.ExportToExcel(result.Value.Items, _entityName);
        return Result.Success(bytes);
    }

    protected virtual Task<Result<byte[]>> GetTemplateAsync<TImportDto>(CancellationToken cancellationToken) where TImportDto : class
    {
        var bytes = ExcelExportHelper.GenerateTemplate<TImportDto>();
        return Task.FromResult(Result.Success(bytes));
    }

    protected async Task<Result<int>> ImportAsync<TImportDto, TResponse>(
        Stream fileStream,
        CancellationToken cancellationToken,
        Func<TImportDto, TEntity?, Task>? beforeSave = null) 
        where TImportDto : class, new()
    {
        try
        {
            var rows = ExcelImportHelper.Import<TImportDto>(fileStream);
            var importedCount = 0;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            foreach (var dto in rows)
            {
                // Try to find existing by Code
                TEntity? existing = null;
                var codeProp = typeof(TImportDto).GetProperty("Code");
                if (codeProp != null)
                {
                    var codeValue = codeProp.GetValue(dto)?.ToString();
                    if (!string.IsNullOrEmpty(codeValue))
                    {
                        existing = await _repository.GetByCodeAsync(codeValue, cancellationToken);
                    }
                }

                if (existing != null)
                {
                    _mapper.Map(dto, existing);
                    if (beforeSave != null) await beforeSave(dto, existing);
                    _repository.Update(existing);
                }
                else
                {
                    var entity = _mapper.Map<TEntity>(dto);
                    if (beforeSave != null) await beforeSave(dto, entity);
                    await _repository.AddAsync(entity, cancellationToken);
                }
                importedCount++;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(importedCount);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure<int>("ImportError", ex.Message);
        }
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
        return Result.Success(new PagedResult<TResponse>(dtos, totalItems, pageIndex, pageSize));
    }

    protected async Task<Result<TResponse>> GetByCodeAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null)
        where TResponse : class, new()
    {
        var query = _repository.AsQueryable();

        // Filter out Soft Deleted IF ModifiedType is mapped
        var modifiedTypeProp = typeof(TEntity).GetProperty("ModifiedType");
        if (modifiedTypeProp != null && !Attribute.IsDefined(modifiedTypeProp, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
        {
            query = query.Where(e => e.ModifiedType != ModificationType.Delete.ToString());
        }
        
        if (includes != null)
            query = includes(query);

        var entity = await query.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
        
        if (entity == null)
            return Result.Failure<TResponse>(Error.NotFound(entityName, code));

        var dto = _mapper.Map<TResponse>(entity);
        
        // Populate child collections (images, details, etc.)
        await PopulateCollectionsAsync(new List<TResponse> { dto }, null, cancellationToken).ConfigureAwait(false);

        return Result.Success(dto);
    }

    protected async Task<Result<TResponse>> GetByCodeIncludeChildrenAsync<TResponse>(
        string code, 
        string entityName,
        CancellationToken cancellationToken,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes)
        where TResponse : class, new()
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
                var name = GetEntityDisplayName(entity);
                names.Add(name);
            }

            return Result.Failure(Error.Conflict(MessageConstants.Conflict,
                $"Cannot delete {typeof(TEntity).Name.Replace("Tbl", "")}s because they have active {dependencyEntityName}: {string.Join(", ", names)}"));
        }

        return Result.Success();
    }

    private string GetEntityDisplayName(TEntity entity)
    {
        var type = entity.GetType();
        var displayNameProps = new[] { "Name", "Username", "FullName", "Title", "Code" };
        
        foreach (var propName in displayNameProps)
        {
            var prop = type.GetProperty(propName);
            if (prop != null)
            {
                var val = prop.GetValue(entity)?.ToString();
                if (!string.IsNullOrEmpty(val)) return val;
            }
        }
        
        return entity.Code;
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

        var entityType = typeof(TEntity);
        var entityProps = entityType.GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        // Safe default sorting
        if (sortDTO == null)
        {
            sortDTO = new SortDTO { Sort = "DESC" };
            // Default to CreatedAt if mapped, else Code
            if (entityProps.TryGetValue("CreatedAt", out var createdAtProp) && 
                !Attribute.IsDefined(createdAtProp, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
            {
                sortDTO.SortBy = "CreatedAt";
            }
            else
            {
                sortDTO.SortBy = "Code";
            }
        }
        else if (string.IsNullOrEmpty(sortDTO.SortBy))
        {
             // If SortBy is null/empty but sortDTO exists
             if (entityProps.TryGetValue("CreatedAt", out var createdAtProp) && 
                !Attribute.IsDefined(createdAtProp, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
            {
                sortDTO.SortBy = "CreatedAt";
            }
            else
            {
                sortDTO.SortBy = "Code";
            }
        }

        if (string.IsNullOrEmpty(sortDTO.Sort)) 
            sortDTO.Sort = sortDTO.SortDescending ? "DESC" : "ASC";

        // Filter out Soft Deleted items by default IF ModifiedType is mapped
        if (!searchFields.Any(s => s.SearchField == nameof(IEntity.ModifiedType)))
        {
            if (entityProps.TryGetValue("ModifiedType", out var modifiedTypeProp) && 
                !Attribute.IsDefined(modifiedTypeProp, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute)))
            {
                searchFields.Add(new SearchDTO 
                { 
                    SearchField = nameof(IEntity.ModifiedType), 
                    SearchCondition = SearchCondition.NotEqual, 
                    SearchValue = ModificationType.Delete.ToString() 
                });
            }
        }
        
        var tableName = typeof(TEntity).Name;
        
        // Auto-discover Reference Tables from TResponse Attributes
        var autoRefTables = GetReferenceTables<TResponse>();
        referenceTables = referenceTables == null 
            ? autoRefTables 
            : referenceTables.Concat(autoRefTables).ToList();

        // Filter out fields that are ReferenceCollections (child lists) as they are not columns in the main table
        // They are populated separately in PopulateCollectionsAsync
        List<string>? sqlFields = FilterAndValidateFields<TResponse>(fields, referenceTables);

        // Build parameterized query (SQL injection safe)
        var queryResult = QueryBuilder.BuildRawQueryPagingParameterized(
            pageSize, pageIndex, tableName, referenceTables, searchFields, sortDTO, sqlFields);
        
        sw.Restart();

        // Execute with parameters
        var dynamicResults = await SqlMapper.QueryAsync<dynamic>(
            connection,
            queryResult.Sql, 
            queryResult.Parameters
        ).ConfigureAwait(false);
        
        // Convert to list
        var resultList = dynamicResults.ToList();
        sw.Restart();

        var dtos = new List<TResponse>();
        int totalCount = 0;

        if (resultList.Any())
        {
            totalCount = Convert.ToInt32(((IDictionary<string, object>)resultList.First())["TotalRow"]);
            dtos = MapDynamicList<TResponse>(resultList);
        }
        
        sw.Restart();
        
        // Populate Collections with ConfigureAwait
        // Pass the original 'fields' (not sqlFields) to control which collections to fetch
        await PopulateCollectionsAsync(dtos, fields, cancellationToken).ConfigureAwait(false);

        return Result.Success(new PagedResult<TResponse>(dtos, totalCount, pageIndex, pageSize));
    }

    protected List<ReferenceTable> GetReferenceTables<TResponse>()
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
        List<string>? fields = null,
        CancellationToken cancellationToken = default) where TResponse : class, new()
    {
        if (items == null || !items.Any()) return;

        using var connection = _dapperContext.CreateConnection();
        connection.Open();
        
        var responseType = typeof(TResponse);
        var props = responseType.GetProperties();

        // Parse fields to dictionary: CollectionName -> Set of Fields
        // e.g. "ProductImages.Name" -> "ProductImages": ["Name"]
        // "ProductImages" -> "ProductImages": [] (means all)
        var collectionFieldsMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        
        if (fields != null && fields.Any())
        {
            foreach (var f in fields)
            {
                var parts = f.Split('.', 2);
                var collectionName = parts[0];
                
                if (!collectionFieldsMap.ContainsKey(collectionName))
                {
                    collectionFieldsMap[collectionName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                if (parts.Length > 1)
                {
                    // Partial selection: string "Name" from "ProductImages.Name"
                    collectionFieldsMap[collectionName].Add(parts[1]);
                }
                else
                {
                    // Full selection: string "ProductImages"
                    // Mark as empty to signify "Select *" or handle as full object
                    collectionFieldsMap[collectionName].Clear(); 
                }
            }
        }

        foreach (var prop in props)
        {
            // Optimization: Skip if fields are specified and this property is not requested
            // Note: If fields contains "ProductImages.Name", key "ProductImages" exists in map.
            if (fields != null && fields.Any() && !collectionFieldsMap.ContainsKey(prop.Name))
            {
                continue;
            }

            var collAttr = prop.GetCustomAttribute<ReferenceCollectionAttribute>();
            if (collAttr == null) continue;

            // Determine which fields to select for this collection
            List<string>? childSelectFields = null;
            if (collectionFieldsMap.TryGetValue(prop.Name, out var requestedChildFields) && requestedChildFields.Any())
            {
                // Partial selection requested
                // MUST include Key and ForeignKey for mapping/joining
                // Primary Key of Child (collAttr.Key)
                // We assume child type has property named same as Key, usually "Code"
                requestedChildFields.Add("Code"); 
                // Foreign Key to Parent (collAttr.ForeignKey)
                requestedChildFields.Add(collAttr.ForeignKey);
                
                childSelectFields = requestedChildFields.ToList();
            }

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
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            if (childSelectFields == null || !childSelectFields.Any())
            {
                if (collAttr.ChildTableName == "TblFile")
                {
                    sb.Append("c.*, c.\"Path\" AS \"ImageURL\"");
                }
                else
                {
                    sb.Append("c.*");
                }
            }
            else
            {
                var selectParts = childSelectFields.Select(f => {
                    var colName = SqlBuilderHelpers.NormalizeFieldName(f);
                    // Handle specific mappings: ImageURL -> Path for TblFile
                    if (collAttr.ChildTableName == "TblFile" && f.Equals("ImageURL", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"c.\"Path\" AS \"{f}\"";
                    }
                    return $"c.\"{colName}\"";
                });
                sb.Append(string.Join(", ", selectParts));
            }

            // Optimized SQL: Use standard column name to allow index usage. 
            // Case-insensitivity is handled by providing both original and lowercase codes in @Codes
            // and using StringComparer.OrdinalIgnoreCase on the C# dictionary.
            sb.Append($" FROM \"{collAttr.ChildTableName}\" AS c WHERE c.\"{collAttr.ForeignKey}\" = ANY(@Codes)");
            
            var sql = sb.ToString();
            
            // Build a list of codes including both original and lowercase versions for robust matching
            var codesToFetch = parentCodes.Concat(parentCodes.Select(c => c.ToLower())).Distinct().ToArray();
            object parameters = new { Codes = codesToFetch, FilterValue = collAttr.FilterValue };

            if (!string.IsNullOrEmpty(collAttr.FilterColumn) && !string.IsNullOrEmpty(collAttr.FilterValue))
            {
                if (collAttr.FilterValue.Contains(","))
                {
                    var values = collAttr.FilterValue.Split(',').Select(v => v.Trim()).ToArray();
                    // Optimization: Use ILIKE for case-insensitive matching
                    sql += $" AND c.\"{collAttr.FilterColumn}\" ILIKE ANY(@FilterValues)";
                    parameters = new { Codes = codesToFetch, FilterValues = values };
                }
                else
                {
                    // Optimization: Use ILIKE for case-insensitive matching
                    sql += $" AND c.\"{collAttr.FilterColumn}\" ILIKE @FilterValue";
                    parameters = new { Codes = codesToFetch, FilterValue = collAttr.FilterValue };
                }
            }
            
            Log.Debug("[BaseHandler] PopulateCollectionsAsync: SQL=\"{Sql}\", Codes={Codes}, Filter={Filter}", sql, string.Join(",", parentCodes), collAttr.FilterValue);
            
            // Execute with ConfigureAwait(false)
            var children = await SqlMapper.QueryAsync<dynamic>(connection, sql, parameters)
                .ConfigureAwait(false);
            
            Log.Debug("[BaseHandler] PopulateCollectionsAsync: Fetched {Count} children for {ParentType}", children.Count(), typeof(TResponse).Name);
            
            // Performance Optimization: Direct Mapping for children instead of JSON
            // Refactored to avoid generic reflection and CS8602 errors
            
            var childDtoType = collAttr.ChildDtoType;
            if (childDtoType == null) continue;

#pragma warning disable CS8602 // Dereference of a possibly null reference
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            // Use the non-generic helper
            var childList = MapDynamicListRuntime(childDtoType, children);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference

            // Group children by foreign key (case-insensitive for robust matching)
            var groupedChildren = new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);
            var foreignKeyProp = childDtoType.GetProperty(collAttr.ForeignKey);
            
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

            foreach (var child in childList)
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

    protected List<T> MapDynamicList<T>(IEnumerable<dynamic> source) where T : new()
    {
        var list = new List<T>();
        var type = typeof(T);
        var props = _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var row in source)
        {
            var dict = (IDictionary<string, object>)row;
            var ciDict = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            var item = new T();
            foreach (var prop in props)
            {
                if (ciDict.TryGetValue(prop.Name, out var value) && value != null && value != DBNull.Value)
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

    /// <summary>
    /// Non-generic version of MapDynamicList to allow runtime type mapping without MakeGenericMethod reflection
    /// </summary>
    private System.Collections.IList MapDynamicListRuntime(Type targetType, dynamic source)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        var listType = typeof(List<>).MakeGenericType(targetType);
        var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
        
        var props = _propertyCache.GetOrAdd(targetType, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var row in source)
        {
            var dict = row as IDictionary<string, object>;
            if (dict == null) continue;
            
            // Create a case-insensitive lookup for the current row's properties
            var ciDict = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            var item = Activator.CreateInstance(targetType)!;
            
            foreach (var prop in props)
            {
                if (ciDict.TryGetValue(prop.Name, out var value) && value != null && value != DBNull.Value)
                {
                    try
                    {
                        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        
                        if (propType.IsEnum)
                        {
                            prop.SetValue(item, Enum.ToObject(propType, Convert.ChangeType(value, Enum.GetUnderlyingType(propType))));
                        }
                        else
                        {
                            prop.SetValue(item, Convert.ChangeType(value, propType));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip - can happen if DB value doesn't match DTO property type
                        System.Diagnostics.Debug.WriteLine($"Error mapping property {prop.Name}: {ex.Message}");
                    }
                }
            }
            list.Add(item);
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference
        return list;
    }
}

/// <summary>
/// Generic Base Handler that implements MediatR IRequestHandler for standard CRUD operations.
/// This allows removing explicit handler files for simple entities.
/// Made abstract to avoid automatic discovery by MediatR assembly scanner which would fail on open generic parameters.
/// </summary>
public class BaseHandler<TEntity, TResponse, TCreateDto, TUpdateDto> : BaseHandler<TEntity>,
    IRequestHandler<GetPagedQuery<TResponse>, Result<PagedResult<TResponse>>>,
    IRequestHandler<GetByCodeQuery<TResponse>, Result<TResponse>>,
    IRequestHandler<CreateCommand<TCreateDto, TResponse>, Result<TResponse>>,
    IRequestHandler<UpdateCommand<TUpdateDto, TResponse>, Result<TResponse>>,
    IRequestHandler<DeleteCommand<TEntity>, Result>,
    IRequestHandler<DeleteMultipleCommand<TEntity>, Result>,
    IRequestHandler<GetStatsQuery<TEntity>, Result<EntityStatsDto>>,
    IRequestHandler<ExportAllQuery<TResponse>, Result<byte[]>>,
    IRequestHandler<GetTemplateQuery<TCreateDto>, Result<byte[]>>,
    IRequestHandler<ImportCommand<TCreateDto, TResponse>, Result<int>>
    where TEntity : class, IEntity
    where TResponse : class, IBaseDto, new()
    where TCreateDto : class, new()
    where TUpdateDto : class
{
    public BaseHandler(
        IRepository<TEntity> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext)
        : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public virtual Task<Result<PagedResult<TResponse>>> Handle(GetPagedQuery<TResponse> request, CancellationToken cancellationToken)
        => base.Handle<TResponse>(request, cancellationToken);

    public virtual Task<Result<TResponse>> Handle(GetByCodeQuery<TResponse> request, CancellationToken cancellationToken)
        => base.Handle<TResponse>(request, cancellationToken);

    public virtual Task<Result<TResponse>> Handle(CreateCommand<TCreateDto, TResponse> request, CancellationToken cancellationToken)
        => CreateAsync<TCreateDto, TResponse>(request.Dto, cancellationToken, c => {
            // Auto-generate code if not provided
            if (string.IsNullOrEmpty(c.Code))
            {
                c.Code = $"{_entityName.Substring(0, Math.Min(3, _entityName.Length)).ToUpper()}{DateTime.Now.Ticks.ToString().Substring(12)}";
            }
            c.IsActive = true;
        });

    public virtual Task<Result<TResponse>> Handle(UpdateCommand<TUpdateDto, TResponse> request, CancellationToken cancellationToken)
        => UpdateAsync<TUpdateDto, TResponse>(request.Code, request.Dto, _entityName, cancellationToken);

    public virtual Task<Result> Handle(DeleteCommand<TEntity> request, CancellationToken cancellationToken)
        => DeleteAsync(request.Code, _entityName, cancellationToken);

    public virtual Task<Result> Handle(DeleteMultipleCommand<TEntity> request, CancellationToken cancellationToken)
        => DeleteMultipleAsync(request.Codes, _entityName, cancellationToken);

    public virtual async Task<Result<EntityStatsDto>> Handle(GetStatsQuery<TEntity> request, CancellationToken cancellationToken)
    {
         var query = _repository.AsQueryable()
                .Where(e => e.ModifiedType != ModificationType.Delete.ToString());

         var total = await query.CountAsync(cancellationToken);
         var active = await query.CountAsync(e => e.IsActive, cancellationToken);
         
         return Result.Success(new EntityStatsDto { Total = total, Active = active });
    }

    public virtual async Task<Result<byte[]>> Handle(ExportAllQuery<TResponse> request, CancellationToken cancellationToken)
    {
        var result = await GetPagedDapperAsync<TResponse>(1, 10000, null, null, null, null, cancellationToken);
        if (result.IsFailure) return Result.Failure<byte[]>(result.Error!);

        // Hook for data refinement (Custom export logic here)
        var exportData = await PrepareDataForExportAsync(result.Value.Items);
        
        var bytes = ExcelExportHelper.ExportToExcel(exportData, _entityName);
        return Result.Success(bytes);
    }

    protected virtual Task<IEnumerable<TResponse>> PrepareDataForExportAsync(IEnumerable<TResponse> data)
    {
        return Task.FromResult(data);
    }

    public virtual Task<Result<byte[]>> Handle(GetTemplateQuery<TCreateDto> request, CancellationToken cancellationToken)
        => GetTemplateAsync<TCreateDto>(cancellationToken);

    public virtual Task<Result<int>> Handle(ImportCommand<TCreateDto, TResponse> request, CancellationToken cancellationToken)
        => base.ImportAsync<TCreateDto, TResponse>(request.FileStream, cancellationToken, OnBeforeImportSaveAsync);

    protected virtual Task OnBeforeImportSaveAsync(TCreateDto dto, TEntity? entity)
    {
        return Task.CompletedTask;
    }
}

