using AutoMapper;
using FW.WAPI.Core.Attribute;
using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Configuration;
using FW.WAPI.Core.DAL.Model.Validation;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.EventBus.Event;
using FW.WAPI.Core.Repository;
using FW.WAPI.Core.Runtime.Session;
using FW.WAPI.Core.Service.IntegrationEventLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static FW.WAPI.Core.General.EnumTypes;

/// <summary>
///
/// </summary>
namespace FW.WAPI.Core.Service
{
    public class BaseService<TDataContext, TEntity> : IBaseService<TDataContext, TEntity>
        where TDataContext : DbContext
        where TEntity : class, new()
    {
        public readonly IRepository<TDataContext, TEntity> _repo;
        public readonly List<TableConfig> listTableConfig;
        private readonly DistributedCompany _distributedCompany;

        private readonly string tableName;
        public readonly IServiceProvider _serviceProvider;
        public readonly IBaseSession _session;
        public readonly DatabaseProvider _databaseProvider;

        public BaseService(IRepository<TDataContext, TEntity> repository,
            IOptions<List<TableConfig>> tabletSettings, IOptions<DistributedCompany> distributedCompany,
            IBaseSession baseSession, IServiceProvider serviceProvider)
        {
            _repo = repository;
            listTableConfig = tabletSettings.Value;
            _distributedCompany = distributedCompany.Value;
            _serviceProvider = serviceProvider;
            _session = baseSession;

            _databaseProvider = _serviceProvider.GetRequiredService<IStartupCoreOptions>().DatabaseProvider;
            tableName = Utilities.GetTableName<TDataContext, TEntity>(_repo.DataContext);
        }

        #region insert data

        /// <summary>
        ///
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Insert(TEntity objectToInsert)
        {
            dynamic objectToInsertAudit = objectToInsert;

            objectToInsertAudit.CreatedDate = DateTime.Now;
            objectToInsertAudit.ModifiedType = ModifiedType.Add.ToString();
            objectToInsertAudit.Scope = objectToInsertAudit.CompanyCode;
            objectToInsertAudit.ScopeType = "All";
            objectToInsertAudit.CreatedAt = "Center";

            objectToInsertAudit.CreatedBy = _session.UserName;

            if (objectToInsertAudit.Code == null)
            {
                objectToInsertAudit.Code = await _repo.GenerateUniqueCode(objectToInsertAudit.CompanyCode);
            }

            //Insert childList
            var propsInfo = (typeof(TEntity)).GetProperties();
            //Check code duplicate
            var primaryKeyAttributeProp = propsInfo.FirstOrDefault(x => x.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null);

            if (primaryKeyAttributeProp != null)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, primaryKeyAttributeProp.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = primaryKeyAttributeProp.GetCustomAttribute<PrimaryKeyAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = primaryKeyAttributeProp.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new FriendlyException("Không được phép trùng mã.");
                    }
                }

            }

            //Check unique column
            var unqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);

            foreach (var item in unqueColAttributeProps)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, item.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = item.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new FriendlyException("Giá trị phải là duy nhất.");
                    }
                }
            }

            using (var scope = _repo.DataContext.Database.BeginTransaction())
            {
                try
                {
                    _repo.Insert(objectToInsertAudit);

                    var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);

                    foreach (var prop in propsCollection)
                    {
                        dynamic childItems = prop.GetValue(objectToInsert);

                        if (childItems != null && childItems.Count > 0)
                        {
                            var genericType = prop.PropertyType.GenericTypeArguments[0];

                            //get child repo
                            var typeDBContext = typeof(TDataContext);
                            var childTypeRepo = typeof(IRepository<,>).MakeGenericType(new Type[] { typeDBContext, genericType });
                            var childRepo = _serviceProvider.GetRequiredService(childTypeRepo);

                            //get childlistattribute
                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);
                            //get foreignkey name
                            var foreignkeyCol = childListAttribute.ForeignKeyCode;

                            //check relationship
                            bool manyToManyRelation = childListAttribute.ManyToManyRelation;

                            //get isIdentity primary key
                            bool isIdentity = childListAttribute.IsIdentity;

                            foreach (dynamic item in childItems)
                            {
                                if (!manyToManyRelation)
                                {
                                    //generate code of child item when identity = false and code is null
                                    if (!isIdentity && item.Code == null)
                                    {
                                        item.Code = Guid.NewGuid().ToString();
                                    }

                                    PropertyInfo propFK = item.GetType().GetProperty(foreignkeyCol);
                                    propFK.SetValue(item, objectToInsertAudit.Code);
                                }
                                else
                                {
                                    var combineKeyName = childListAttribute.CombineKey;

                                    PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                    propCombine.SetValue(item, objectToInsertAudit.Code);
                                }

                                item.ModifiedType = ModifiedType.Add.ToString();
                                item.Scope = objectToInsertAudit.CompanyCode;
                                item.ScopeType = "All";
                                item.CreatedAt = "Center";
                                item.CreatedBy = _session.UserName;
                                item.CompanyCode = objectToInsertAudit.CompanyCode;
                                item.CreatedDate = DateTime.Now;

                            }

                            Task result = (Task)childRepo.GetType().GetMethod("InsertRangeAsync").Invoke(childRepo, new object[] { childItems });
                            await result;
                        }
                    }

                    var resultCount = await _repo.SaveChangesAsync();

                    if (resultCount == 0)
                    {
                        throw new FriendlyException("Insert fail.");
                    }

                    scope.Commit();
                }
                catch
                {
                    throw;
                }
            }

            return objectToInsert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Insert(TEntity objectToInsert, IntegrationEvent @event, string createdBy = null, EventState eventState = EventState.NotPublished)
        {
            dynamic objectToInsertAudit = objectToInsert;

            objectToInsertAudit.CreatedDate = DateTime.Now;
            objectToInsertAudit.ModifiedType = ModifiedType.Add.ToString();
            objectToInsertAudit.Scope = objectToInsertAudit.CompanyCode;
            objectToInsertAudit.ScopeType = "All";
            objectToInsertAudit.CreatedAt = "Center";
            objectToInsertAudit.CreatedBy = createdBy == null ? _session.UserName : createdBy;

            if (objectToInsertAudit.Code == null)
            {
                objectToInsertAudit.Code = await _repo.GenerateUniqueCode(objectToInsertAudit.CompanyCode);
            }

            var propsInfo = (typeof(TEntity)).GetProperties();

            var primaryKeyAttributeProp = propsInfo.FirstOrDefault(x => x.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null);

            if (primaryKeyAttributeProp != null)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, primaryKeyAttributeProp.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = primaryKeyAttributeProp.GetCustomAttribute<PrimaryKeyAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = primaryKeyAttributeProp.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new DuplicateCodeException("Không được phép trùng mã.");
                    }
                }

            }

            //Check unique column
            var unqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);
            foreach (var item in unqueColAttributeProps)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, item.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = item.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new FriendlyException("Giá trị phải là duy nhất.");
                    }
                }
            }

            using (var scope = _repo.DataContext.Database.BeginTransaction())
            {
                try
                {
                    _repo.Insert(objectToInsertAudit);

                    //Insert childList
                    var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);

                    foreach (var prop in propsCollection)
                    {
                        dynamic childItems = prop.GetValue(objectToInsert);

                        if (childItems != null && childItems.Count > 0)
                        {
                            var genericType = prop.PropertyType.GenericTypeArguments[0];

                            //get child repo
                            var typeDBContext = typeof(TDataContext);
                            var childTypeRepo = typeof(IRepository<,>).MakeGenericType(new Type[] { typeDBContext, genericType });
                            var childRepo = _serviceProvider.GetRequiredService(childTypeRepo);

                            //get childlistattribute
                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);
                            //get foreignkey name
                            var foreignkeyCol = childListAttribute.ForeignKeyCode;

                            //check relationship
                            bool manyToManyRelation = childListAttribute.ManyToManyRelation;

                            //get isIdentity primary key
                            bool isIdentity = childListAttribute.IsIdentity;

                            foreach (dynamic item in childItems)
                            {
                                if (!manyToManyRelation)
                                {
                                    //generate code of child item when identity = false and code is null
                                    if (!isIdentity && item.Code == null)
                                    {
                                        item.Code = Guid.NewGuid().ToString();
                                    }

                                    PropertyInfo propFK = item.GetType().GetProperty(foreignkeyCol);
                                    propFK.SetValue(item, objectToInsertAudit.Code);
                                }
                                else
                                {
                                    var combineKeyName = childListAttribute.CombineKey;
                                    //childListAttribute.ConstructorArguments[1].Value.ToString();

                                    PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                    propCombine.SetValue(item, objectToInsertAudit.Code);
                                }

                                item.ModifiedType = ModifiedType.Add.ToString();
                                item.Scope = objectToInsertAudit.CompanyCode;
                                item.ScopeType = "All";
                                item.CreatedAt = "Center";
                                item.CreatedBy = createdBy == null ? _session.UserName : createdBy;
                                item.CompanyCode = objectToInsertAudit.CompanyCode;
                                item.CreatedDate = DateTime.Now;

                            }

                            Task result = (Task)childRepo.GetType().GetMethod("InsertRangeAsync").Invoke(childRepo, new object[] { childItems });
                            await result;
                        }
                    }

                    var resultCount = await _repo.SaveChangesAsync();

                    if (resultCount == 0)
                    {
                        throw new FriendlyException("Insert fail.");
                    }

                    IIntegrationEventLogService integrationEventLogService = _serviceProvider.GetRequiredService<IIntegrationEventLogService>();

                    if (eventState == EventState.NotPublished)
                    {
                        await integrationEventLogService.SaveEventAsync(@event, scope);
                    }
                    else if (eventState == EventState.ProcessCompleted)
                    {
                        await integrationEventLogService.SaveEventProcessComplete(@event, scope);
                    }

                    scope.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return objectToInsert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Insert(TEntity objectToInsert, Func<Task> function)
        {
            dynamic objectToInsertAudit = objectToInsert;

            objectToInsertAudit.CreatedDate = DateTime.Now;
            objectToInsertAudit.ModifiedType = ModifiedType.Add.ToString();
            objectToInsertAudit.Scope = objectToInsertAudit.CompanyCode;
            objectToInsertAudit.ScopeType = "All"; //TODO: hanlde hardcore ScopeType
            objectToInsertAudit.CreatedAt = "Center"; //TODO: hanlde hardcore Created At

            objectToInsertAudit.CreatedBy = _session.UserName;

            if (objectToInsertAudit.Code == null)
            {
                objectToInsertAudit.Code = await _repo.GenerateUniqueCode(objectToInsertAudit.CompanyCode);
            }

            var propsInfo = (typeof(TEntity)).GetProperties();

            var primaryKeyAttributeProp = propsInfo.FirstOrDefault(x => x.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null);

            if (primaryKeyAttributeProp != null)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, primaryKeyAttributeProp.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = primaryKeyAttributeProp.GetCustomAttribute<PrimaryKeyAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = primaryKeyAttributeProp.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = primaryKeyAttributeProp.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new DuplicateCodeException("Không được phép trùng mã.");
                    }
                }

            }

            //Check unique column
            var unqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);
            foreach (var item in unqueColAttributeProps)
            {
                var val = PropertyUtilities.GetPropValueOfObject(objectToInsertAudit, item.Name);
                if (val != null)
                {
                    var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                    var searchs = new List<SearchDTO>();
                    searchs.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = item.Name,
                        SearchValue = val
                    });

                    if (primaryKeyAttribute.CompanyCode)
                    {
                        searchs.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToInsertAudit.CompanyCode
                        });
                    }

                    var querySearch = "";
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                            break;
                        default:
                            break;
                    }

                    var existItem = await _repo.RawQuerySingleAsync(querySearch);

                    if (existItem != null)
                    {
                        throw new FriendlyException("Giá trị phải là duy nhất.");
                    }
                }
            }

            using (var scope = _repo.DataContext.Database.BeginTransaction())
            {
                try
                {
                    _repo.Insert(objectToInsertAudit);

                    //Insert childList                  
                    var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);

                    foreach (var prop in propsCollection)
                    {
                        dynamic childItems = prop.GetValue(objectToInsert);

                        if (childItems != null && childItems.Count > 0)
                        {
                            var genericType = prop.PropertyType.GenericTypeArguments[0];

                            //get child repo
                            var typeDBContext = typeof(TDataContext);
                            var childTypeRepo = typeof(IRepository<,>).MakeGenericType(new Type[] { typeDBContext, genericType });
                            var childRepo = _serviceProvider.GetRequiredService(childTypeRepo);

                            //get childlistattribute
                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);
                            //get foreignkey name
                            var foreignkeyCol = childListAttribute.ForeignKeyCode;

                            //check relationship
                            bool manyToManyRelation = childListAttribute.ManyToManyRelation;

                            //get isIdentity primary key
                            bool isIdentity = childListAttribute.IsIdentity;

                            foreach (dynamic item in childItems)
                            {
                                if (!manyToManyRelation)
                                {
                                    //generate code of child item when identity = false and code is null
                                    if (!isIdentity && item.Code == null)
                                    {
                                        item.Code = Guid.NewGuid().ToString();
                                    }

                                    PropertyInfo propFK = item.GetType().GetProperty(foreignkeyCol);
                                    propFK.SetValue(item, objectToInsertAudit.Code);
                                }
                                else
                                {
                                    var combineKeyName = childListAttribute.CombineKey;

                                    PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                    propCombine.SetValue(item, objectToInsertAudit.Code);
                                }

                                item.ModifiedType = ModifiedType.Add.ToString();
                                item.Scope = objectToInsertAudit.CompanyCode;
                                item.ScopeType = "All";
                                item.CreatedAt = "Center";
                                item.CreatedBy = _session.UserName;
                                item.CompanyCode = objectToInsertAudit.CompanyCode;
                                item.CreatedDate = DateTime.Now;
                            }

                            Task result = (Task)childRepo.GetType().GetMethod("InsertRangeAsync").Invoke(childRepo, new object[] { childItems });
                            await result;
                        }
                    }

                    var resultCount = await _repo.SaveChangesAsync();

                    if (resultCount == 0)
                    {
                        throw new FriendlyException("Insert fail.");
                    }

                    await function();

                    scope.Commit();
                }
                catch
                {
                    throw new FriendlyException("Insert fail.");
                }
            }

            return objectToInsert;
        }

        #endregion insert data

        #region update data

        /// <summary>
        ///
        /// </summary>
        /// <param name="objectToUpdate"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity objectToUpdate, bool includeChildren = true)
        {
            try
            {
                if (objectToUpdate != null)
                {
                    using (var scope = _repo.DataContext.Database.BeginTransaction())
                    {
                        dynamic objectToUpdateAudit = objectToUpdate;

                        List<SearchDTO> conditions = new List<SearchDTO>()
                        {   new SearchDTO() { SearchField = TableColumnConst.COMPANY_CODE_COL, SearchValue = objectToUpdateAudit.CompanyCode,
                            SearchCondition = SearchCondition.Equal },

                            new SearchDTO(){ SearchField = TableColumnConst.CODE_COL, SearchValue =  objectToUpdateAudit.Code,
                                SearchCondition = SearchCondition.Equal},

                            new SearchDTO(){ SearchField = TableColumnConst.MODIFIED_TYPE_COL, SearchValue = ModifiedType.Delete.ToString(),
                                SearchCondition = SearchCondition.NotEqual}};

                        var expressionFunc = EntityExpression.GetWhereExp(conditions, objectToUpdate);
                        dynamic existObject = _repo.Get(expressionFunc);

                        if (existObject == null)
                        {
                            throw new FriendlyException("Item not exist to update.");
                        }

                        var propsInfo = (typeof(TEntity)).GetProperties();

                        var uniqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);

                        foreach (var item in uniqueColAttributeProps)
                        {
                            var val = PropertyUtilities.GetPropValueOfObject(objectToUpdate, item.Name);
                            var oldVal = PropertyUtilities.GetPropValueOfObject(existObject, item.Name);

                            if (val == oldVal)
                            {
                                continue;
                            }

                            if (val != null)
                            {
                                var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                                var searchs = new List<SearchDTO>();
                                searchs.Add(new SearchDTO()
                                {
                                    SearchCondition = SearchCondition.Equal,
                                    SearchField = item.Name,
                                    SearchValue = val
                                });

                                if (primaryKeyAttribute.CompanyCode)
                                {
                                    var companyCodeVal = PropertyUtilities.GetPropValueOfObject(objectToUpdate, TableColumnConst.COMPANY_CODE_COL);
                                    if (companyCodeVal != null)
                                    {
                                        searchs.Add(new SearchDTO()
                                        {
                                            SearchCondition = SearchCondition.Equal,
                                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                                            SearchValue = companyCodeVal
                                        });
                                    }
                                }

                                var querySearch = "";
                                switch (_databaseProvider)
                                {
                                    case DatabaseProvider.MSSQL:
                                        querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    case DatabaseProvider.POSTGRESQL:
                                        querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    default:
                                        break;
                                }

                                var existItem = await _repo.RawQuerySingleAsync(querySearch);

                                if (existItem != null)
                                {
                                    throw new FriendlyException("Giá trị phải là duy nhất.");
                                }
                            }
                        }

                        objectToUpdateAudit.ModifiedDate = DateTime.Now;
                        objectToUpdateAudit.ModifiedType = ModifiedType.Update.ToString();
                        objectToUpdateAudit.Scope = objectToUpdateAudit.CompanyCode;
                        objectToUpdateAudit.ScopeType = "All";
                        objectToUpdateAudit.CreatedAt = "Center";
                        objectToUpdateAudit.ModifiedBy = _session.UserName;

                        objectToUpdateAudit.CreatedDate = existObject.CreatedDate;
                        objectToUpdateAudit.CreatedBy = existObject.CreatedBy;

                        if (existObject != null)
                        {
                            _repo.Update(existObject, objectToUpdateAudit);

                            if (includeChildren)
                            {
                                //get childlist with type is collection
                                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType &&
                                    x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                                    && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);
                                //get dbcontext
                                var context = _repo.DataContext;

                                foreach (var prop in propsCollection)
                                {
                                    dynamic insertChildItems = prop.GetValue(objectToUpdateAudit);

                                    if (insertChildItems != null)
                                    {
                                        var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                                        if (childListAttribute != null)
                                        {
                                            var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                            bool manyToManyRelation = childListAttribute.ManyToManyRelation;

                                            bool isIdentity = childListAttribute.IsIdentity;

                                            var genericType = prop.PropertyType.GenericTypeArguments[0];

                                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                                            var childSet = tableProp.GetValue(context);

                                            //get existing children items
                                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                                            List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                                SearchValue = existObject.CompanyCode,
                                                SearchCondition = SearchCondition.Equal
                                            });

                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                                                SearchValue = ModifiedType.Delete.ToString(),
                                                SearchCondition = SearchCondition.NotEqual
                                            });

                                            if (!manyToManyRelation)
                                            {
                                                conditionsChildren.Add(new SearchDTO()
                                                {
                                                    SearchField = foreignkeyTableName,
                                                    SearchValue = existObject.Code,
                                                    SearchCondition = SearchCondition.Equal
                                                });
                                            }
                                            else
                                            {
                                                var key = childListAttribute.CombineKey;

                                                conditionsChildren.Add(new SearchDTO()
                                                {
                                                    SearchField = key,
                                                    SearchValue = existObject.Code,
                                                    SearchCondition = SearchCondition.Equal
                                                });
                                            }

                                            var childTableName = Utilities.GetTableName(context, childClassType);

                                            var query = "";

                                            if (manyToManyRelation)
                                            {
                                                switch (_databaseProvider)
                                                {
                                                    case DatabaseProvider.MSSQL:
                                                        query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                        {
                                                            SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                            Sort = "asc"
                                                        });
                                                        break;
                                                    case DatabaseProvider.POSTGRESQL:
                                                        query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                        {
                                                            SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                            Sort = "asc"
                                                        });
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                switch (_databaseProvider)
                                                {
                                                    case DatabaseProvider.MSSQL:
                                                        query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                        break;

                                                    case DatabaseProvider.POSTGRESQL:
                                                        query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                            //find existing childrenitems
                                            var rawQueryMultipleMethod = _repo.GetType().
                                                GetMethod("RawQueryMultiple", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(childClassType);

                                            dynamic existChildItems = rawQueryMultipleMethod.Invoke(_repo, new object[] { query, null });

                                            if (manyToManyRelation)
                                            {
                                                //delete old value
                                                foreach (var childToUpdate in existChildItems)
                                                {
                                                    childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                                                    childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                                    context.Entry(childToUpdate).State = EntityState.Deleted;
                                                }

                                                //insert value
                                                foreach (dynamic item in insertChildItems)
                                                {
                                                    var combineKeyName = prop.GetCustomAttributesData().
                                                        FirstOrDefault(x => x.AttributeType.Name == "ChildListAttribute").ConstructorArguments[1].Value.ToString();

                                                    PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                                    propCombine.SetValue(item, objectToUpdateAudit.Code);

                                                    item.ModifiedType = ModifiedType.Update.ToString();
                                                    item.Scope = objectToUpdateAudit.CompanyCode;
                                                    item.ScopeType = "All";
                                                    item.CreatedAt = "Center";
                                                    item.CreatedBy = _session.UserName;
                                                    item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                    item.ModifiedDate = DateTime.Now;
                                                    item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                    childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                                }
                                            }
                                            else
                                            {
                                                //find children item to delete
                                                List<dynamic> childrenItemDelete = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                                ((IEnumerable<dynamic>)insertChildItems).All(p => p.Code != x.Code)).ToList();

                                                if (childrenItemDelete.Count > 0)
                                                {
                                                    foreach (dynamic childDelete in childrenItemDelete)
                                                    {
                                                        childDelete.ModifiedType = ModifiedType.Delete.ToString();
                                                        childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childDelete });
                                                        context.Entry(childDelete).State = EntityState.Deleted;
                                                    }
                                                }

                                                //find chidlren item to add
                                                List<dynamic> childrenItemAdd = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                                    x.Code == null).ToList();
                                                //Add new children item
                                                if (childrenItemAdd.Count > 0)
                                                {
                                                    foreach (var item in childrenItemAdd)
                                                    {
                                                        if (!isIdentity && item.Code == null)
                                                        {
                                                            item.Code = Guid.NewGuid().ToString();
                                                        }

                                                        PropertyInfo propFK = item.GetType().GetProperty(foreignkeyTableName);
                                                        propFK.SetValue(item, objectToUpdateAudit.Code);

                                                        item.ModifiedType = ModifiedType.Update.ToString();
                                                        item.Scope = objectToUpdateAudit.CompanyCode;
                                                        item.ScopeType = "All";
                                                        item.CreatedAt = "Center";
                                                        item.CreatedBy = _session.UserName;
                                                        item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                        item.ModifiedDate = DateTime.Now;
                                                        item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                        childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                                    }
                                                }

                                                //find children item to update
                                                var childrenItemUpdateSource = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                                ((IEnumerable<dynamic>)insertChildItems).Any(p => p.Code == x.Code)).ToList();

                                                var childrentItemUpdateDestination = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                               ((IEnumerable<dynamic>)existChildItems).Any(p => p.Code == x.Code)).ToList();

                                                foreach (var item in childrentItemUpdateDestination)
                                                {
                                                    var sourceItem = ((IEnumerable<dynamic>)childrenItemUpdateSource).FirstOrDefault(x =>
                                                        x.Code == item.Code);
                                                    var itemUpdate = Utilities.CopyObject(childClassType, item, sourceItem, false);

                                                    if (itemUpdate != null)
                                                    {
                                                        childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { itemUpdate });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            await _repo.SaveChangesAsync();
                            scope.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objectToUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToUpdate"></param>
        /// <param name="event"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity objectToUpdate, IntegrationEvent @event, bool includeChildren = true, EventState eventState = EventState.NotPublished)
        {
            try
            {
                if (objectToUpdate != null)
                {
                    using (var scope = _repo.DataContext.Database.BeginTransaction())
                    {
                        dynamic objectToUpdateAudit = objectToUpdate;

                        List<SearchDTO> conditions = new List<SearchDTO>()
                        {   new SearchDTO() { SearchField = TableColumnConst.COMPANY_CODE_COL, SearchValue = objectToUpdateAudit.CompanyCode,
                            SearchCondition = SearchCondition.Equal },

                            new SearchDTO(){ SearchField = TableColumnConst.CODE_COL, SearchValue =  objectToUpdateAudit.Code,
                                SearchCondition = SearchCondition.Equal},

                            new SearchDTO(){ SearchField = TableColumnConst.MODIFIED_TYPE_COL, SearchValue = ModifiedType.Delete.ToString(),
                                SearchCondition = SearchCondition.NotEqual}};

                        var expressionFunc = EntityExpression.GetWhereExp(conditions, objectToUpdate);
                        dynamic existObject = _repo.Get(expressionFunc);

                        if (existObject == null)
                        {
                            throw new FriendlyException("Item not exist to update.");
                        }

                        var propsInfo = (typeof(TEntity)).GetProperties();

                        var unqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);

                        foreach (var item in unqueColAttributeProps)
                        {
                            var val = PropertyUtilities.GetPropValueOfObject(objectToUpdate, item.Name);
                            var oldVal = PropertyUtilities.GetPropValueOfObject(existObject, item.Name);

                            if (val == oldVal)
                            {
                                continue;
                            }

                            if (val != null)
                            {
                                var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                                var searchs = new List<SearchDTO>();
                                searchs.Add(new SearchDTO()
                                {
                                    SearchCondition = SearchCondition.Equal,
                                    SearchField = item.Name,
                                    SearchValue = val
                                });

                                if (primaryKeyAttribute.CompanyCode)
                                {
                                    var companyCodeVal = PropertyUtilities.GetPropValueOfObject(objectToUpdate, TableColumnConst.COMPANY_CODE_COL);
                                    if (companyCodeVal != null)
                                    {
                                        searchs.Add(new SearchDTO()
                                        {
                                            SearchCondition = SearchCondition.Equal,
                                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                                            SearchValue = companyCodeVal
                                        });
                                    }
                                }

                                var querySearch = "";
                                switch (_databaseProvider)
                                {
                                    case DatabaseProvider.MSSQL:
                                        querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    case DatabaseProvider.POSTGRESQL:
                                        querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    default:
                                        break;
                                }

                                var existItem = await _repo.RawQuerySingleAsync(querySearch);

                                if (existItem != null)
                                {
                                    throw new FriendlyException("Giá trị phải là duy nhất.");
                                }
                            }
                        }

                        objectToUpdateAudit.ModifiedDate = DateTime.Now;
                        objectToUpdateAudit.ModifiedType = ModifiedType.Update.ToString();
                        objectToUpdateAudit.Scope = objectToUpdateAudit.CompanyCode;
                        objectToUpdateAudit.ScopeType = "All";
                        objectToUpdateAudit.CreatedAt = "Center";
                        objectToUpdateAudit.ModifiedBy = _session.UserName;
                        objectToUpdateAudit.CreatedDate = existObject.CreatedDate;
                        objectToUpdateAudit.CreatedBy = existObject.CreatedBy;

                        if (existObject != null)
                        {
                            _repo.Update(existObject, objectToUpdateAudit);

                            if (includeChildren)
                            {
                                //get childlist with type is collection
                                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType &&
                                    x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                                    && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);
                                //get dbcontext
                                var context = _repo.DataContext;

                                foreach (var prop in propsCollection)
                                {
                                    dynamic insertChildItems = prop.GetValue(objectToUpdateAudit);

                                    if (insertChildItems != null)
                                    {
                                        var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                                        if (childListAttribute != null)
                                        {
                                            var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                            bool manyToManyRelation = childListAttribute.ManyToManyRelation;
                                            bool isIdentity = childListAttribute.IsIdentity;

                                            var genericType = prop.PropertyType.GenericTypeArguments[0];

                                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                                            var childSet = tableProp.GetValue(context);

                                            //get existing children items
                                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                                            List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                                SearchValue = existObject.CompanyCode,
                                                SearchCondition = SearchCondition.Equal
                                            });

                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                                                SearchValue = ModifiedType.Delete.ToString(),
                                                SearchCondition = SearchCondition.NotEqual
                                            });

                                            if (!manyToManyRelation)
                                            {
                                                conditionsChildren.Add(new SearchDTO()
                                                {
                                                    SearchField = foreignkeyTableName,
                                                    SearchValue = existObject.Code,
                                                    SearchCondition = SearchCondition.Equal
                                                });
                                            }
                                            else
                                            {
                                                var key = childListAttribute.CombineKey;

                                                conditionsChildren.Add(new SearchDTO()
                                                {
                                                    SearchField = key,
                                                    SearchValue = existObject.Code,
                                                    SearchCondition = SearchCondition.Equal
                                                });
                                            }

                                            var childTableName = Utilities.GetTableName(context, childClassType);

                                            var query = "";

                                            if (manyToManyRelation)
                                            {
                                                switch (_databaseProvider)
                                                {
                                                    case DatabaseProvider.MSSQL:
                                                        query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                        {
                                                            SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                            Sort = "asc"
                                                        });
                                                        break;
                                                    case DatabaseProvider.POSTGRESQL:
                                                        query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                        {
                                                            SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                            Sort = "asc"
                                                        });
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                switch (_databaseProvider)
                                                {
                                                    case DatabaseProvider.MSSQL:
                                                        query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                        break;

                                                    case DatabaseProvider.POSTGRESQL:
                                                        query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                            //find existing childrenitems
                                            var rawQueryMultipleMethod = _repo.GetType().
                                                GetMethod("RawQueryMultiple", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(childClassType);

                                            dynamic existChildItems = rawQueryMultipleMethod.Invoke(_repo, new object[] { query, null });

                                            if (manyToManyRelation)
                                            {
                                                //delete old value
                                                foreach (var childToUpdate in existChildItems)
                                                {
                                                    childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                                                    childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                                    context.Entry(childToUpdate).State = EntityState.Deleted;
                                                }

                                                //insert value
                                                foreach (dynamic item in insertChildItems)
                                                {
                                                    var combineKeyName = prop.GetCustomAttributesData().
                                                        FirstOrDefault(x => x.AttributeType.Name == "ChildListAttribute").ConstructorArguments[1].Value.ToString();

                                                    PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                                    propCombine.SetValue(item, objectToUpdateAudit.Code);

                                                    item.ModifiedType = ModifiedType.Update.ToString();
                                                    item.Scope = objectToUpdateAudit.CompanyCode;
                                                    item.ScopeType = "All";
                                                    item.CreatedAt = "Center";
                                                    item.CreatedBy = _session.UserName;
                                                    item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                    item.ModifiedDate = DateTime.Now;
                                                    item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                    childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                                }
                                            }
                                            else
                                            {
                                                //find children item to delete
                                                List<dynamic> childrenItemDelete = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                                ((IEnumerable<dynamic>)insertChildItems).All(p => p.Code != x.Code)).ToList();

                                                if (childrenItemDelete.Count > 0)
                                                {
                                                    foreach (dynamic childDelete in childrenItemDelete)
                                                    {
                                                        childDelete.ModifiedType = ModifiedType.Delete.ToString();
                                                        childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childDelete });
                                                        context.Entry(childDelete).State = EntityState.Deleted;
                                                    }
                                                }

                                                //find chidlren item to add
                                                List<dynamic> childrenItemAdd = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                                    x.Code == null).ToList();
                                                //Add new children item
                                                if (childrenItemAdd.Count > 0)
                                                {
                                                    foreach (var item in childrenItemAdd)
                                                    {
                                                        if (!isIdentity && item.Code == null)
                                                        {
                                                            item.Code = Guid.NewGuid().ToString();
                                                        }

                                                        PropertyInfo propFK = item.GetType().GetProperty(foreignkeyTableName);
                                                        propFK.SetValue(item, objectToUpdateAudit.Code);

                                                        item.ModifiedType = ModifiedType.Update.ToString();
                                                        item.Scope = objectToUpdateAudit.CompanyCode;
                                                        item.ScopeType = "All";
                                                        item.CreatedAt = "Center";
                                                        item.CreatedBy = _session.UserName;
                                                        item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                        item.ModifiedDate = DateTime.Now;
                                                        item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                        childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                                    }
                                                }

                                                //find children item to update
                                                var childrenItemUpdateSource = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                                ((IEnumerable<dynamic>)insertChildItems).Any(p => p.Code == x.Code)).ToList();

                                                var childrentItemUpdateDestination = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                               ((IEnumerable<dynamic>)existChildItems).Any(p => p.Code == x.Code)).ToList();

                                                foreach (var item in childrentItemUpdateDestination)
                                                {
                                                    var sourceItem = ((IEnumerable<dynamic>)childrenItemUpdateSource).FirstOrDefault(x =>
                                                        x.Code == item.Code);
                                                    var itemUpdate = Utilities.CopyObject(childClassType, item, sourceItem, false);

                                                    if (itemUpdate != null)
                                                    {
                                                        childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { itemUpdate });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            await _repo.SaveChangesAsync();

                            IIntegrationEventLogService integrationEventLogService = _serviceProvider.GetRequiredService<IIntegrationEventLogService>();

                            if (eventState == EventState.NotPublished)
                            {
                                await integrationEventLogService.SaveEventAsync(@event, scope);
                            }
                            else if (eventState == EventState.ProcessCompleted)
                            {
                                await integrationEventLogService.SaveEventProcessComplete(@event, scope);
                            }

                            scope.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objectToUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToUpdate"></param>
        /// <param name="action"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity objectToUpdate, Func<Task> action, bool includeChildren = true)
        {
            try
            {
                if (objectToUpdate != null)
                {
                    using (var scope = _repo.DataContext.Database.BeginTransaction())
                    {
                        dynamic objectToUpdateAudit = objectToUpdate;

                        objectToUpdateAudit.ModifiedDate = DateTime.Now;
                        objectToUpdateAudit.ModifiedType = ModifiedType.Update.ToString();
                        objectToUpdateAudit.Scope = objectToUpdateAudit.CompanyCode;
                        objectToUpdateAudit.ScopeType = "All";
                        objectToUpdateAudit.CreatedAt = "Center";
                        objectToUpdateAudit.ModifiedBy = _session.UserName;

                        var propsInfo = (typeof(TEntity)).GetProperties();

                        List<SearchDTO> conditions = new List<SearchDTO>()
                        {   new SearchDTO() { SearchField = TableColumnConst.COMPANY_CODE_COL, SearchValue = objectToUpdateAudit.CompanyCode,
                            SearchCondition = SearchCondition.Equal },

                            new SearchDTO(){ SearchField = TableColumnConst.CODE_COL, SearchValue =  objectToUpdateAudit.Code,
                                SearchCondition = SearchCondition.Equal},

                            new SearchDTO(){ SearchField = TableColumnConst.MODIFIED_TYPE_COL, SearchValue = ModifiedType.Delete.ToString(),
                                SearchCondition = SearchCondition.NotEqual}};

                        var expressionFunc = EntityExpression.GetWhereExp(conditions, objectToUpdate);
                        dynamic existObject = _repo.Get(expressionFunc);

                        if (existObject == null)
                        {
                            throw new FriendlyException("Item not exist to update.");
                        }

                        var uniqueColAttributeProps = propsInfo.Where(x => x.GetCustomAttribute(typeof(UniqueAttribute)) != null);

                        foreach (var item in uniqueColAttributeProps)
                        {
                            var val = PropertyUtilities.GetPropValueOfObject(objectToUpdate, item.Name);
                            var oldVal = PropertyUtilities.GetPropValueOfObject(existObject, item.Name);

                            if (val == oldVal)
                            {
                                continue;
                            }

                            if (val != null)
                            {
                                var primaryKeyAttribute = item.GetCustomAttribute<UniqueAttribute>();
                                var searchs = new List<SearchDTO>();
                                searchs.Add(new SearchDTO()
                                {
                                    SearchCondition = SearchCondition.Equal,
                                    SearchField = item.Name,
                                    SearchValue = val
                                });

                                if (primaryKeyAttribute.CompanyCode)
                                {
                                    var companyCodeVal = PropertyUtilities.GetPropValueOfObject(objectToUpdate, TableColumnConst.COMPANY_CODE_COL);
                                    if (companyCodeVal != null)
                                    {
                                        searchs.Add(new SearchDTO()
                                        {
                                            SearchCondition = SearchCondition.Equal,
                                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                                            SearchValue = companyCodeVal
                                        });
                                    }
                                }

                                var querySearch = "";
                                switch (_databaseProvider)
                                {
                                    case DatabaseProvider.MSSQL:
                                        querySearch = SqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    case DatabaseProvider.POSTGRESQL:
                                        querySearch = PostgresSqlUtilities.BuildRawQuery(tableName, searchs, new SortDTO() { SortBy = item.Name, Sort = "asc" });
                                        break;
                                    default:
                                        break;
                                }

                                var existItem = await _repo.RawQuerySingleAsync(querySearch);

                                if (existItem != null)
                                {
                                    throw new FriendlyException("Giá trị phải là duy nhất.");
                                }
                            }
                        }

                        objectToUpdateAudit.CreatedDate = existObject.CreatedDate;
                        objectToUpdateAudit.CreatedBy = _session.UserName;


                        _repo.Update(existObject, objectToUpdateAudit);

                        if (includeChildren)
                        {
                            //get childlist with type is collection
                            var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType &&
                                x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                                && x.GetCustomAttribute(typeof(ChildListAttribute)) != null);
                            //get dbcontext
                            var context = _repo.DataContext;

                            foreach (var prop in propsCollection)
                            {
                                dynamic insertChildItems = prop.GetValue(objectToUpdateAudit);

                                if (insertChildItems != null)
                                {
                                    var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                                    if (childListAttribute != null)
                                    {
                                        var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                        bool manyToManyRelation = childListAttribute.ManyToManyRelation;
                                        bool isIdentity = childListAttribute.IsIdentity;

                                        var genericType = prop.PropertyType.GenericTypeArguments[0];

                                        var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                                        var childSet = tableProp.GetValue(context);

                                        //get existing children items
                                        Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                                        conditionsChildren.Add(new SearchDTO()
                                        {
                                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                                            SearchValue = existObject.CompanyCode,
                                            SearchCondition = SearchCondition.Equal
                                        });

                                        conditionsChildren.Add(new SearchDTO()
                                        {
                                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                                            SearchValue = ModifiedType.Delete.ToString(),
                                            SearchCondition = SearchCondition.NotEqual
                                        });

                                        if (!manyToManyRelation)
                                        {
                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = foreignkeyTableName,
                                                SearchValue = existObject.Code,
                                                SearchCondition = SearchCondition.Equal
                                            });
                                        }
                                        else
                                        {
                                            var key = childListAttribute.CombineKey;

                                            conditionsChildren.Add(new SearchDTO()
                                            {
                                                SearchField = key,
                                                SearchValue = existObject.Code,
                                                SearchCondition = SearchCondition.Equal
                                            });
                                        }

                                        var childTableName = Utilities.GetTableName(context, childClassType);

                                        var query = "";

                                        if (manyToManyRelation)
                                        {
                                            switch (_databaseProvider)
                                            {
                                                case DatabaseProvider.MSSQL:
                                                    query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                    {
                                                        SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                        Sort = "asc"
                                                    });
                                                    break;
                                                case DatabaseProvider.POSTGRESQL:
                                                    query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, new SortDTO()
                                                    {
                                                        SortBy = TableColumnConst.COMPANY_CODE_COL,
                                                        Sort = "asc"
                                                    });
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (_databaseProvider)
                                            {
                                                case DatabaseProvider.MSSQL:
                                                    query = SqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                    break;

                                                case DatabaseProvider.POSTGRESQL:
                                                    query = PostgresSqlUtilities.BuildRawQuery(childTableName, conditionsChildren, null);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }

                                        //find existing childrenitems
                                        var rawQueryMultipleMethod = _repo.GetType().
                                            GetMethod("RawQueryMultiple", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(childClassType);

                                        dynamic existChildItems = rawQueryMultipleMethod.Invoke(_repo, new object[] { query, null });

                                        if (manyToManyRelation)
                                        {
                                            //delete old value
                                            foreach (var childToUpdate in existChildItems)
                                            {
                                                childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                                                childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                                context.Entry(childToUpdate).State = EntityState.Deleted;
                                            }

                                            //insert value
                                            foreach (dynamic item in insertChildItems)
                                            {
                                                var combineKeyName = prop.GetCustomAttributesData().
                                                    FirstOrDefault(x => x.AttributeType.Name == "ChildListAttribute").ConstructorArguments[1].Value.ToString();

                                                PropertyInfo propCombine = item.GetType().GetProperty(combineKeyName);
                                                propCombine.SetValue(item, objectToUpdateAudit.Code);

                                                item.ModifiedType = ModifiedType.Update.ToString();
                                                item.Scope = objectToUpdateAudit.CompanyCode;
                                                item.ScopeType = "All";
                                                item.CreatedAt = "Center";
                                                item.CreatedBy = _session.UserName;
                                                item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                item.ModifiedDate = DateTime.Now;
                                                item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                            }
                                        }
                                        else
                                        {
                                            //find children item to delete
                                            List<dynamic> childrenItemDelete = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                            ((IEnumerable<dynamic>)insertChildItems).All(p => p.Code != x.Code)).ToList();

                                            if (childrenItemDelete.Count > 0)
                                            {
                                                foreach (dynamic childDelete in childrenItemDelete)
                                                {
                                                    childDelete.ModifiedType = ModifiedType.Delete.ToString();
                                                    childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childDelete });
                                                    context.Entry(childDelete).State = EntityState.Deleted;
                                                }
                                            }

                                            //find chidlren item to add
                                            List<dynamic> childrenItemAdd = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                                x.Code == null).ToList();
                                            //Add new children item
                                            if (childrenItemAdd.Count > 0)
                                            {
                                                foreach (var item in childrenItemAdd)
                                                {
                                                    if (!isIdentity && item.Code == null)
                                                    {
                                                        item.Code = Guid.NewGuid().ToString();
                                                    }

                                                    PropertyInfo propFK = item.GetType().GetProperty(foreignkeyTableName);
                                                    propFK.SetValue(item, objectToUpdateAudit.Code);

                                                    item.ModifiedType = ModifiedType.Update.ToString();
                                                    item.Scope = objectToUpdateAudit.CompanyCode;
                                                    item.ScopeType = "All";
                                                    item.CreatedAt = "Center";
                                                    item.CreatedBy = _session.UserName;
                                                    item.CompanyCode = objectToUpdateAudit.CompanyCode;
                                                    item.ModifiedDate = DateTime.Now;
                                                    item.CreatedDate = objectToUpdateAudit.CreatedDate;

                                                    childSet.GetType().GetMethod("Add").Invoke(childSet, new object[] { item });
                                                }
                                            }

                                            //find children item to update
                                            var childrenItemUpdateSource = ((IEnumerable<dynamic>)existChildItems).Where(x =>
                                            ((IEnumerable<dynamic>)insertChildItems).Any(p => p.Code == x.Code)).ToList();

                                            var childrentItemUpdateDestination = ((IEnumerable<dynamic>)insertChildItems).Where(x =>
                                           ((IEnumerable<dynamic>)existChildItems).Any(p => p.Code == x.Code)).ToList();

                                            foreach (var item in childrentItemUpdateDestination)
                                            {
                                                var sourceItem = ((IEnumerable<dynamic>)childrenItemUpdateSource).FirstOrDefault(x =>
                                                    x.Code == item.Code);
                                                var itemUpdate = Utilities.CopyObject(childClassType, item, sourceItem, false);

                                                if (itemUpdate != null)
                                                {
                                                    childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { itemUpdate });
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            await _repo.SaveChangesAsync();

                            await action();
                            scope.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objectToUpdate;
        }

        #endregion update data

        #region delete data

        /// <summary>
        /// update data modifyType column to 'Delete'
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Delete(object code, object companyCode = null)
        {
            var IsSuccess = false;

            List<SearchDTO> conditions = new List<SearchDTO>();

            if (companyCode != null)
            {
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.COMPANY_CODE_COL,
                    SearchValue = companyCode,
                    SearchCondition = SearchCondition.Equal
                });
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.CODE_COL,
                    SearchValue = code,
                    SearchCondition = SearchCondition.Equal
                });
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                    SearchValue = ModifiedType.Delete.ToString(),
                    SearchCondition = SearchCondition.NotEqual
                });
            }
            else
            {
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.CODE_COL,
                    SearchValue = code,
                    SearchCondition = SearchCondition.Equal
                });

                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                    SearchValue = ModifiedType.Delete.ToString(),
                    SearchCondition = SearchCondition.NotEqual
                });
            }

            var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

            dynamic objectToDelete = await _repo.GetAsync(funcExpression);

            if (objectToDelete == null)
            {
                throw new FriendlyException("Not found item to delete.");
            }

            if (objectToDelete is IValidationEntity validationEntity)
            {
                var validation = validationEntity.Validate();

                if (validation.Any())
                {
                    var err = "";
                    foreach (var messErr in validation)
                    {
                        err += messErr.ErrorMessage + "; ";
                    }

                    throw new FriendlyException(err);
                }
            }

            using (var scope = _repo.DataContext.Database.BeginTransaction())
            {
                try
                {
                    objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                    _repo.Update(objectToDelete);

                    var propsInfo = (typeof(TEntity)).GetProperties();
                    //get childlist with type is collection
                    var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                        && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                    //get dbcontext
                    var context = _repo.DataContext;

                    List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                    foreach (var prop in propsCollection)
                    {
                        conditionsChildren.Clear();

                        var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);
                        //get cascadedelete
                        bool cascadeDelete = childListAttribute.CascadeDelete;

                        //if cascadedelete = false; countinue the loop
                        if (!cascadeDelete)
                        {
                            continue;
                        }

                        //check relationship
                        bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                        Type parentType = prop.PropertyType;
                        var genericType = parentType.GenericTypeArguments[0];

                        var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                        var childSet = tableProp.GetValue(context);

                        //update child collection to modifytype = delete
                        Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                        if (isManyToManyRelationship)
                        {
                            var combineKeyName = childListAttribute.CombineKey;

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = combineKeyName,
                                SearchValue = objectToDelete.Code,
                                SearchCondition = SearchCondition.Equal
                            });
                        }
                        else
                        {
                            var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = foreignkeyTableName,
                                SearchValue = objectToDelete.Code,
                                SearchCondition = SearchCondition.Equal
                            });
                        }

                        conditionsChildren.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = objectToDelete.CompanyCode,
                            SearchCondition = SearchCondition.Equal
                        });

                        var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                            BindingFlags.Public).MakeGenericMethod(childClassType);
                        dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                        var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                        foreach (dynamic childToUpdate in result)
                        {
                            childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                            childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { childToUpdate });
                        }
                    }

                    await _repo.SaveChangesAsync();
                    scope.Commit();
                    IsSuccess = true;
                }
                catch
                {
                    IsSuccess = false;
                }
            }

            return IsSuccess;
        }

        /// <summary>
        /// Remove data from database
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Remove(object code, object companyCode = null)
        {
            var IsSuccess = false;
            try
            {
                using (var scope = _repo.DataContext.Database.BeginTransaction())
                {
                    List<SearchDTO> conditions = new List<SearchDTO>();

                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }

                    var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

                    var objectToDelete = _repo.Get(funcExpression) as dynamic;

                    if (objectToDelete != null)
                    {
                        if (objectToDelete is IValidationEntity validationEntity)
                        {
                            var validation = validationEntity.Validate();

                            if (validation.Any())
                            {
                                var err = "";
                                foreach (var messErr in validation)
                                {
                                    err += messErr.ErrorMessage + "; ";
                                }

                                throw new FriendlyException(err);
                            }
                        }

                        objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                        _repo.Delete(objectToDelete);

                        var propsInfo = (typeof(TEntity)).GetProperties();
                        //get childlist with type is collection
                        var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                        //get dbcontext
                        var context = _repo.DataContext;

                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                        foreach (var prop in propsCollection)
                        {
                            conditionsChildren.Clear();

                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                            //get cascadedelete
                            bool cascadeDelete = childListAttribute.CascadeDelete;
                            //if cascadedelete = false; countinue the loop
                            if (!cascadeDelete)
                            {
                                continue;
                            }

                            //check relationship
                            bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                            Type parentType = prop.PropertyType;
                            var genericType = parentType.GenericTypeArguments[0];

                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                            var childSet = tableProp.GetValue(context);

                            //delete childs
                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                            if (isManyToManyRelationship)
                            {
                                var combineKeyName = childListAttribute.CombineKey;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = combineKeyName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }
                            else
                            {
                                var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = foreignkeyTableName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                SearchValue = objectToDelete.CompanyCode,
                                SearchCondition = SearchCondition.Equal
                            });

                            var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                                BindingFlags.Public).MakeGenericMethod(childClassType);
                            dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                            var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                            foreach (dynamic childToUpdate in result)
                            {
                                childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                context.Entry(childToUpdate).State = EntityState.Deleted;
                            }
                        }

                        await _repo.SaveChangesAsync();
                        scope.Commit();
                        IsSuccess = true;
                    }
                }
            }
            catch
            {
                throw;
            }

            return IsSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="event"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Delete(object code, IntegrationEvent @event, object companyCode = null, EventState eventState = EventState.NotPublished)
        {
            var IsSuccess = false;

            try
            {
                using (var scope = _repo.DataContext.Database.BeginTransaction())
                {
                    List<SearchDTO> conditions = new List<SearchDTO>();

                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }

                    var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

                    dynamic objectToDelete = await _repo.GetAsync(funcExpression);

                    if (objectToDelete != null)
                    {
                        if (objectToDelete is IValidationEntity validationEntity)
                        {
                            var validation = validationEntity.Validate();

                            if (validation.Any())
                            {
                                var err = "";
                                foreach (var messErr in validation)
                                {
                                    err += messErr.ErrorMessage + "; ";
                                }

                                throw new FriendlyException(err);
                            }
                        }

                        objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                        _repo.Update(objectToDelete);

                        var propsInfo = (typeof(TEntity)).GetProperties();
                        //get childlist with type is collection
                        var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                        //get dbcontext
                        var context = _repo.DataContext;

                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                        foreach (var prop in propsCollection)
                        {
                            conditionsChildren.Clear();

                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                            //get cascadedelete
                            bool cascadeDelete = childListAttribute.CascadeDelete;
                            //if cascadedelete = false; countinue the loop
                            if (!cascadeDelete)
                            {
                                continue;
                            }

                            //check relationship
                            bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                            Type parentType = prop.PropertyType;
                            var genericType = parentType.GenericTypeArguments[0];

                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                            var childSet = tableProp.GetValue(context);

                            //update child collection to modifytype = delete
                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                            if (isManyToManyRelationship)
                            {
                                var combineKeyName = childListAttribute.CombineKey;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = combineKeyName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }
                            else
                            {
                                var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = foreignkeyTableName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                SearchValue = objectToDelete.CompanyCode,
                                SearchCondition = SearchCondition.Equal
                            });

                            var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                                BindingFlags.Public).MakeGenericMethod(childClassType);
                            dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                            var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                            foreach (dynamic childToUpdate in result)
                            {
                                childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                                childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { childToUpdate });
                            }
                        }

                        await _repo.SaveChangesAsync();

                        IIntegrationEventLogService integrationEventLogService = _serviceProvider.GetRequiredService<IIntegrationEventLogService>();

                        if (eventState == EventState.NotPublished)
                        {
                            await integrationEventLogService.SaveEventAsync(@event, scope);
                        }
                        else if (eventState == EventState.ProcessCompleted)
                        {
                            await integrationEventLogService.SaveEventProcessComplete(@event, scope);
                        }

                        scope.Commit();
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return IsSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="event"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Remove(object code, IntegrationEvent @event, object companyCode = null, EventState eventState = EventState.NotPublished)
        {
            var IsSuccess = false;
            try
            {
                using (var scope = _repo.DataContext.Database.BeginTransaction())
                {
                    List<SearchDTO> conditions = new List<SearchDTO>();

                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }

                    var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

                    var objectToDelete = _repo.Get(funcExpression) as dynamic;

                    if (objectToDelete != null)
                    {
                        if (objectToDelete is IValidationEntity validationEntity)
                        {
                            var validation = validationEntity.Validate();

                            if (validation.Any())
                            {
                                var err = "";
                                foreach (var messErr in validation)
                                {
                                    err += messErr.ErrorMessage + "; ";
                                }

                                throw new FriendlyException(err);
                            }
                        }

                        objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                        _repo.Delete(objectToDelete);

                        var propsInfo = (typeof(TEntity)).GetProperties();
                        //get childlist with type is collection
                        var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                        //get dbcontext
                        var context = _repo.DataContext;

                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                        foreach (var prop in propsCollection)
                        {
                            conditionsChildren.Clear();

                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                            //get cascadedelete
                            bool cascadeDelete = childListAttribute.CascadeDelete;
                            //if cascadedelete = false; countinue the loop
                            if (!cascadeDelete)
                            {
                                continue;
                            }

                            //check relationship
                            bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                            Type parentType = prop.PropertyType;
                            var genericType = parentType.GenericTypeArguments[0];

                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                            var childSet = tableProp.GetValue(context);

                            //delete childs
                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                            if (isManyToManyRelationship)
                            {
                                var combineKeyName = childListAttribute.CombineKey;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = combineKeyName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }
                            else
                            {
                                var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = foreignkeyTableName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                SearchValue = objectToDelete.CompanyCode,
                                SearchCondition = SearchCondition.Equal
                            });

                            var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                                BindingFlags.Public).MakeGenericMethod(childClassType);
                            dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                            var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                            foreach (dynamic childToUpdate in result)
                            {
                                childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                context.Entry(childToUpdate).State = EntityState.Deleted;
                            }
                        }

                        await _repo.SaveChangesAsync();

                        IIntegrationEventLogService integrationEventLogService = _serviceProvider.GetRequiredService<IIntegrationEventLogService>();

                        if (eventState == EventState.NotPublished)
                        {
                            await integrationEventLogService.SaveEventAsync(@event, scope);
                        }
                        else if (eventState == EventState.ProcessCompleted)
                        {
                            await integrationEventLogService.SaveEventProcessComplete(@event, scope);
                        }

                        scope.Commit();
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return IsSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="action"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Delete(object code, Func<Task> action, object companyCode = null)
        {
            var IsSuccess = false;


            using (var scope = _repo.DataContext.Database.BeginTransaction())
            {
                List<SearchDTO> conditions = new List<SearchDTO>();

                if (companyCode != null)
                {
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = companyCode,
                        SearchCondition = SearchCondition.Equal
                    });
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.CODE_COL,
                        SearchValue = code,
                        SearchCondition = SearchCondition.Equal
                    });
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                        SearchValue = ModifiedType.Delete.ToString(),
                        SearchCondition = SearchCondition.NotEqual
                    });
                }
                else
                {
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.CODE_COL,
                        SearchValue = code,
                        SearchCondition = SearchCondition.Equal
                    });

                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                        SearchValue = ModifiedType.Delete.ToString(),
                        SearchCondition = SearchCondition.NotEqual
                    });
                }

                var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

                dynamic objectToDelete = await _repo.GetAsync(funcExpression);

                if (objectToDelete != null)
                {
                    if (objectToDelete is IValidationEntity validationEntity)
                    {
                        var validation = validationEntity.Validate();

                        if (validation.Any())
                        {
                            var err = "";
                            foreach (var messErr in validation)
                            {
                                err += messErr.ErrorMessage + "; ";
                            }

                            throw new FriendlyException(err);
                        }
                    }

                    try
                    {
                        objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                        _repo.Update(objectToDelete);

                        var propsInfo = (typeof(TEntity)).GetProperties();
                        //get childlist with type is collection
                        var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                        //get dbcontext
                        var context = _repo.DataContext;

                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                        foreach (var prop in propsCollection)
                        {
                            conditionsChildren.Clear();

                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                            //get cascadedelete
                            bool cascadeDelete = childListAttribute.CascadeDelete;
                            //if cascadedelete = false; countinue the loop
                            if (!cascadeDelete)
                            {
                                continue;
                            }

                            //check relationship
                            bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                            Type parentType = prop.PropertyType;
                            var genericType = parentType.GenericTypeArguments[0];

                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                            var childSet = tableProp.GetValue(context);

                            //update child collection to modifytype = delete
                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                            if (isManyToManyRelationship)
                            {
                                var combineKeyName = childListAttribute.CombineKey;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = combineKeyName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }
                            else
                            {
                                var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = foreignkeyTableName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                SearchValue = objectToDelete.CompanyCode,
                                SearchCondition = SearchCondition.Equal
                            });

                            var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                                BindingFlags.Public).MakeGenericMethod(childClassType);
                            dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                            var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                            foreach (dynamic childToUpdate in result)
                            {
                                childToUpdate.ModifiedType = ModifiedType.Delete.ToString();
                                childSet.GetType().GetMethod("Update").Invoke(childSet, new object[] { childToUpdate });
                            }
                        }

                        await _repo.SaveChangesAsync();

                        await action();
                        scope.Commit();
                        IsSuccess = true;
                    }
                    catch
                    {
                        IsSuccess = false;
                    }
                }
            }

            return IsSuccess;
        }

        /// <summary>
        /// Allow pass task function as parameter before commit remove
        /// </summary>
        /// <param name="code"></param>
        /// <param name="action"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<bool> Remove(object code, Func<Task> action, object companyCode = null)
        {
            var IsSuccess = false;
            try
            {
                using (var scope = _repo.DataContext.Database.BeginTransaction())
                {
                    List<SearchDTO> conditions = new List<SearchDTO>();

                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.CODE_COL,
                            SearchValue = code,
                            SearchCondition = SearchCondition.Equal
                        });
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                            SearchValue = ModifiedType.Delete.ToString(),
                            SearchCondition = SearchCondition.NotEqual
                        });
                    }

                    var funcExpression = EntityExpression.GetWhereExp(conditions, new TEntity());

                    var objectToDelete = _repo.Get(funcExpression) as dynamic;

                    if (objectToDelete != null)
                    {
                        if (objectToDelete is IValidationEntity validationEntity)
                        {
                            var validation = validationEntity.Validate();

                            if (validation.Any())
                            {
                                var err = "";
                                foreach (var messErr in validation)
                                {
                                    err += messErr.ErrorMessage + "; ";
                                }

                                throw new FriendlyException(err);
                            }
                        }

                        objectToDelete.ModifiedType = ModifiedType.Delete.ToString();
                        _repo.Delete(objectToDelete);

                        var propsInfo = (typeof(TEntity)).GetProperties();
                        //get childlist with type is collection
                        var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));
                        //get dbcontext
                        var context = _repo.DataContext;

                        List<SearchDTO> conditionsChildren = new List<SearchDTO>();

                        foreach (var prop in propsCollection)
                        {
                            conditionsChildren.Clear();

                            var childListAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

                            //get cascadedelete
                            bool cascadeDelete = childListAttribute.CascadeDelete;
                            //if cascadedelete = false; countinue the loop
                            if (!cascadeDelete)
                            {
                                continue;
                            }

                            //check relationship
                            bool isManyToManyRelationship = childListAttribute.ManyToManyRelation;

                            Type parentType = prop.PropertyType;
                            var genericType = parentType.GenericTypeArguments[0];

                            var tableProp = context.GetType().GetProperties().FirstOrDefault(x => x.Name == genericType.Name);
                            var childSet = tableProp.GetValue(context);

                            //delete childs
                            Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                            if (isManyToManyRelationship)
                            {
                                var combineKeyName = childListAttribute.CombineKey;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = combineKeyName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }
                            else
                            {
                                var foreignkeyTableName = childListAttribute.ForeignKeyCode;

                                conditionsChildren.Add(new SearchDTO()
                                {
                                    SearchField = foreignkeyTableName,
                                    SearchValue = objectToDelete.Code,
                                    SearchCondition = SearchCondition.Equal
                                });
                            }

                            conditionsChildren.Add(new SearchDTO()
                            {
                                SearchField = TableColumnConst.COMPANY_CODE_COL,
                                SearchValue = objectToDelete.CompanyCode,
                                SearchCondition = SearchCondition.Equal
                            });

                            var method = typeof(EntityExpression).GetMethod("GetWhereExpByType", BindingFlags.Static |
                                BindingFlags.Public).MakeGenericMethod(childClassType);
                            dynamic func = method.Invoke(null, new object[] { conditionsChildren, childClassType });

                            var result = ((IEnumerable<object>)Queryable.Where((dynamic)childSet, func)).ToList();

                            foreach (dynamic childToUpdate in result)
                            {
                                childSet.GetType().GetMethod("Remove").Invoke(childSet, new object[] { childToUpdate });
                                context.Entry(childToUpdate).State = EntityState.Deleted;
                            }
                        }

                        await _repo.SaveChangesAsync();
                        await action();
                        scope.Commit();
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return IsSuccess;
        }
        #endregion delete data

        #region query data

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="sortDTO"></param>
        /// <param name="dataLevel"></param>
        /// <returns></returns>
        [ObsoleteAttribute("This method will soon be deprecated.Please use GetAllAsync method.")]
        public virtual IEnumerable<TEntity> GetAll(object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            List<SearchDTO> conditions = BuildSearchConditions(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                default:
                    break;
            }

            IEnumerable<TEntity> results = _repo.RawQueryMultiple<TEntity>(rawQuery, tenantCode);

            return results;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            List<TEntity> results;

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                default:
                    break;
            }

            IEnumerable<TEntity> tmpResutls = await _repo.RawQueryMultipleAsync<TEntity>(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<OneToOneAttribute>(false) != null);

            if (propOneToOnes.Any())
            {
                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceOneToOneValue(item, itemProp);
                        itemProp.SetValue(item, propValue);
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceSumValue(item, itemProp);
                        var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                        itemProp.SetValue(item, exactValue);
                    }
                }
            }

            results = tmpResutls.ToList();
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="companyCode"></param>
        /// <param name="sortDTO"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<object> GetAllFieldsAsync(List<string> fields, object companyCode = null, SortDTO sortDTO = null,
          DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            var validateFields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(validateFields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, validateFields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, validateFields);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

            if (propOneToOnes.Any())
            {
                propOneToOnes = propOneToOnes.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceOneToOneValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                propSum = propSum.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceSumValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }
            return tmpResutls;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllIncludeChildrenAsync(object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {

            List<SearchDTO> conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(null);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, null);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, null);
                    break;
                default:
                    break;
            }

            IEnumerable<TEntity> tmpResutls = await _repo.RawQueryMultipleAsync<TEntity>(rawQuery, tenantCode);
            var results = tmpResutls.ToList();

            var propOneToOnes = typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<OneToOneAttribute>(false) != null);

            if (propOneToOnes.Any())
            {
                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceOneToOneValue(item, itemProp);
                        itemProp.SetValue(item, propValue);
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceSumValue(item, itemProp);
                        var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                        itemProp.SetValue(item, exactValue);
                    }
                }
            }

            var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                && x.GetCustomAttribute(typeof(ChildListAttribute)) != null).ToList();

            foreach (var prop in propsCollection)
            {
                foreach (var item in results)
                {
                    var data = GetChildListObject(prop, item, true, fields, tenantCode);
                    prop.SetValue(item, data);
                }
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="companyCode"></param>
        /// <param name="sortDTO"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<object> GetAllIncludeChildrenFieldsAsync(List<string> fields, object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);
            List<SearchDTO> conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, sortDTO, fields);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                && x.GetCustomAttribute(typeof(ChildListAttribute)) != null).ToList();


            foreach (var prop in propsCollection)
            {
                var propName = prop.Name;
                foreach (var item in tmpResutls)
                {
                    var resutlDictionary = item as IDictionary<string, object>;

                    var defaultAll = propName + ".All";

                    var isAllField = fields.Any(x => x == defaultAll);

                    if (isAllField)
                    {
                        var data = await GetChildListObjectAsync(prop, item, true, null, tenantCode);

                        resutlDictionary[prop.Name] = data;
                    }
                    else
                    {
                        var childFields = fields.Where(x => x.Contains(prop.Name) && x != defaultAll).ToList();
                        if (childFields.Count > 0)
                        {
                            var childFieldsName = childFields.Select(x => x.Replace(propName + ".", "")).ToList();
                            var data = await GetChildListObjectAsync(prop, item, true, childFieldsName, tenantCode);

                            resutlDictionary[prop.Name] = data;
                        }
                    }
                }
            }

            return tmpResutls;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<TEntity>> GetAllPagingAsync(int pageIndex, int pageSize, object companyCode = null,
            SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            var result = new ResultDTO<TEntity>();


            List<SearchDTO> conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(null);
            var rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList,
                        conditions, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList,
                        conditions, sortDTO);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryMultipleDynamicAsync(rawQuery, tenantCode);

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    var config = new MapperConfiguration(cfg => { });
                    var mapper = config.CreateMapper();

                    IList<TEntity> mapObjects = mapper.Map<IList<dynamic>, IList<TEntity>>(tmpResutls);

                    result.Data = mapObjects;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }
            else
            {
                result.Data = null;
                result.Total = 0;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="companyCode"></param>
        /// <param name="sortDTO"></param>
        /// <param name="dataLevel"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<object>> GetAllPagingFieldsAsync(int pageIndex, int pageSize, List<string> fields, object companyCode = null,
          SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            var result = new ResultDTO<object>();

            List<SearchDTO> conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);
            var rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList,
                        conditions, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList,
                        conditions, sortDTO, fields);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    foreach (IDictionary<string, object> item in tmpResutls)
                    {
                        item.Remove("TotalRow");
                    }

                    result.Data = tmpResutls;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }
            else
            {
                result.Data = null;
                result.Total = 0;
            }


            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<TEntity>> GetAllPagingIncludeChildren(int pageIndex, int pageSize, object companyCode = null,
            SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            var result = new ResultDTO<TEntity>();


            List<SearchDTO> conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList, conditions, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize, pageIndex, tableName, refTblList, conditions, sortDTO);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryMultipleDynamicAsync(rawQuery, tenantCode);

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    var config = new MapperConfiguration(cfg => { });
                    var mapper = config.CreateMapper();

                    var mapObjects = mapper.Map<IList<dynamic>, IList<TEntity>>(tmpResutls);

                    var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                        x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)).ToList();

                    propsCollection.ForEach(prop =>
                    {
                        foreach (var item in mapObjects)
                        {
                            var data = GetChildListObject(prop, item);
                            prop.SetValue(item, data);
                        }
                    });

                    result.Data = mapObjects;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByCode(object code, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {

            List<SearchDTO> conditions = BuildSearchConditions(tableName, companyCode, dataLevel, tenantCode);

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = code,
                SearchCondition = SearchCondition.Equal
            });

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQuerySingleAsync(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<OneToOneAttribute>(false) != null);

            if (propOneToOnes.Any())
            {
                foreach (var itemProp in propOneToOnes)
                {
                    var propValue = await GetReferenceOneToOneValue(result, itemProp);
                    itemProp.SetValue(result, propValue);
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                foreach (var itemProp in propSum)
                {
                    var propValue = await GetReferenceSumValue(result, itemProp);
                    var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                    itemProp.SetValue(result, exactValue);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns>Only one row</returns>
        public virtual async Task<object> GetByCodeFields(object code, object companyCode = null,
           DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            List<SearchDTO> conditions = BuildSearchConditions(tableName, companyCode, dataLevel, tenantCode);

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = code,
                SearchCondition = SearchCondition.Equal
            });

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQueryFirstAsync(rawQuery, tenantCode);

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual TEntity GetByCodeIncludeChildren(object code, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {
            List<SearchDTO> conditions = BuildSearchConditions(tableName, companyCode, dataLevel, tenantCode);

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = code,
                SearchCondition = SearchCondition.Equal
            });

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQuery(tableName, conditions, null, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQuery(tableName, conditions, null, fields);
                    break;
                default:
                    break;
            }

            var result = _repo.RawQuerySingle(rawQuery, tenantCode);

            if (result != null)
            {
                var propsInfo = (typeof(TEntity)).GetProperties();

                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                    && x.GetCustomAttribute<ChildListAttribute>(false) != null);

                foreach (var prop in propsCollection)
                {
                    var data = GetChildListObject(prop, result);
                    prop.SetValue(result, data);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByCodeIncludeChildrenAsync(object code, object companyCode = null,
            List<string> fields = null, string tenantCode = null)
        {
            List<SearchDTO> conditions = new List<SearchDTO>();

            //check company code
            if (companyCode != null)
            {
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.COMPANY_CODE_COL,
                    SearchValue = companyCode,
                    SearchCondition = SearchCondition.Equal
                });
            }

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = code,
                SearchCondition = SearchCondition.Equal
            });

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                SearchValue = ModifiedType.Delete.ToString(),
                SearchCondition = SearchCondition.NotEqual
            });

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQuerySingleAsync(rawQuery, tenantCode);

            if (result != null)
            {
                var propsInfo = (typeof(TEntity)).GetProperties();

                var propOneToOnes = propsInfo.Where(x => x.GetCustomAttribute<OneToOneAttribute>(false) != null);

                if (propOneToOnes.Any())
                {
                    foreach (var itemProp in propOneToOnes)
                    {
                        var propValue = await GetReferenceOneToOneValue(result, itemProp);
                        itemProp.SetValue(result, propValue);
                    }
                }

                var propSum = propsInfo.Where(x => x.GetCustomAttribute<SumAttribute>() != null);

                if (propSum.Any())
                {
                    foreach (var itemProp in propSum)
                    {
                        var propValue = await GetReferenceSumValue(result, itemProp);
                        var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                        itemProp.SetValue(result, exactValue);
                    }
                }

                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                    x.GetCustomAttribute<ChildListAttribute>(false) != null);

                foreach (var prop in propsCollection)
                {
                    var data = GetChildListObject(prop, result, false, null, tenantCode);
                    prop.SetValue(result, data);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<object> GetByCodeIncludeChildrenFieldAsync(object code, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY,
          List<string> fields = null, string tenantCode = null)
        {
            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            var rootfields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            var result = await GetByCodeFields(code, companyCode, dataLevel, rootfields, tenantCode);
            var resutlDictionary = result as IDictionary<string, object>;

            if (result != null)
            {
                var propOneToOnes = typeof(TEntity).GetProperties().
                    Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

                if (propOneToOnes.Any())
                {
                    propOneToOnes = propOneToOnes.Where(p => fields.Contains(p.Name));

                    foreach (var itemProp in propOneToOnes)
                    {
                        var propValue = await GetReferenceOneToOneValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }

                var propSum = typeof(TEntity).GetProperties().
                  Where(x => x.GetCustomAttribute<SumAttribute>() != null);

                if (propSum.Any())
                {
                    propSum = propSum.Where(p => fields.Contains(p.Name));

                    foreach (var itemProp in propSum)
                    {
                        var propValue = await GetReferenceSumValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }

                var propsInfo = (typeof(TEntity)).GetProperties();

                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                    x.GetCustomAttribute(typeof(ChildListAttribute)) != null);

                foreach (var prop in propsCollection)
                {
                    var defaultAll = prop.Name + ".All";

                    var isAllField = fields.Any(x => x == defaultAll);

                    if (isAllField)
                    {
                        var data = await GetChildListObjectAsync(prop, result, true, null, tenantCode);

                        resutlDictionary[prop.Name] = data;
                    }
                    else
                    {
                        var childFields = fields.Where(x => x.Contains(prop.Name) && x != defaultAll).ToList();
                        if (childFields.Count > 0)
                        {
                            var childFieldsName = childFields.Select(x => x.Replace(prop.Name + ".", "")).ToList();
                            var data = await GetChildListObjectAsync(prop, result, true, childFieldsName, tenantCode);

                            resutlDictionary[prop.Name] = data;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<object> GetByCodeIncludeChildrenFieldsAsync(object code, object companyCode = null,
          List<string> fields = null, string tenantCode = null)
        {
            List<SearchDTO> conditions = new List<SearchDTO>();

            //check company code
            if (companyCode != null)
            {
                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.COMPANY_CODE_COL,
                    SearchValue = companyCode,
                    SearchCondition = SearchCondition.Equal
                });
            }

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = code,
                SearchCondition = SearchCondition.Equal
            });

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                SearchValue = ModifiedType.Delete.ToString(),
                SearchCondition = SearchCondition.NotEqual
            });

            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);
            List<string> allFields = fields;

            List<ReferenceTable> refTblList = GetReferenceColumns(allFields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, allFields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, conditions, null, fields);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQueryFirstAsync(rawQuery, tenantCode);
            var resultDictionary = result as IDictionary<string, object>;

            if (result != null)
            {
                var propsInfo = (typeof(TEntity)).GetProperties();

                var propsCollection = propsInfo.Where(x => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));

                foreach (var prop in propsCollection)
                {
                    var propName = prop.Name;

                    if (allFields != null)
                    {
                        var subFields = allFields.Where(x => x.Contains(propName)).ToList();

                        if (subFields.Count > 0)
                        {
                            var data = await GetChildListObjectAsync(prop, result, false, subFields, tenantCode);
                            resultDictionary[propName] = data;
                        }
                        else
                        {
                            var data = await GetChildListObjectAsync(prop, result, false, null, tenantCode);
                            resultDictionary[propName] = data;
                        }
                    }
                    else
                    {
                        var data = await GetChildListObjectAsync(prop, result, false, null, tenantCode);
                        resultDictionary[propName] = data;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> Search(List<SearchDTO> searchList, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY,
            List<string> fields = null, string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryMultipleAsync<TEntity>(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

            if (propOneToOnes.Any())
            {
                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceOneToOneValue(item, itemProp);
                        itemProp.SetValue(item, propValue);
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceSumValue(item, itemProp);
                        var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                        itemProp.SetValue(item, exactValue);
                    }
                }
            }

            return tmpResutls.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<object> SearchFieldsAysnc(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null, int limit = 0)
        {
            var rootfields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(rootfields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO, rootfields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO, rootfields, limit);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

            if (propOneToOnes.Any())
            {
                propOneToOnes = propOneToOnes.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceOneToOneValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                propSum = propSum.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceSumValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }

            return tmpResutls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> SearchFieldsAysnc<T>(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null, int limit = 0)
        {
            var tmpResult = await SearchFieldsAysnc(searchList, fields, sortDTO, companyCode, dataLevel, tenantCode, limit);
            if (tmpResult != null)
                return Utilities.ConvertObjectToObject<IEnumerable<T>>(tmpResult);
            else return Enumerable.Empty<T>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> SearchIncludeChildren(List<SearchDTO> searchList, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY,
            List<string> fields = null, string tenantCode = null)
        {

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryMultipleAsync<TEntity>(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().
                Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

            if (propOneToOnes.Any())
            {
                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceOneToOneValue(item, itemProp);
                        itemProp.SetValue(item, propValue);
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var propValue = await GetReferenceSumValue(item, itemProp);
                        //itemProp.SetValue(item, propValue);\
                        var exactValue = Convert.ChangeType(propValue, itemProp.PropertyType);
                        itemProp.SetValue(item, exactValue);
                    }
                }
            }

            var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                             x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                             && x.GetCustomAttribute<ChildListAttribute>(false) != null).ToList();

            if (propsCollection.Count > 0)
            {
                propsCollection.ForEach(prop =>
                {
                    foreach (var item in tmpResutls)
                    {
                        var data = GetChildListObject(prop, item, false, null, tenantCode);
                        prop.SetValue(item, data);
                    }
                });
            }

            return tmpResutls;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<object> SearchIncludeChildrenFieldAsync(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
         object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY
         , string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }
            //Check valid fields
            var rootfields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(rootfields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO, rootfields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableName, refTblList, searchList, sortDTO, rootfields);
                    break;
                default:
                    break;
            }

            var tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            var propOneToOnes = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<OneToOneAttribute>() != null);

            if (propOneToOnes.Any())
            {
                propOneToOnes = propOneToOnes.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propOneToOnes)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceOneToOneValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }

            var propSum = typeof(TEntity).GetProperties().
              Where(x => x.GetCustomAttribute<SumAttribute>() != null);

            if (propSum.Any())
            {
                propSum = propSum.Where(p => fields.Contains(p.Name));

                foreach (var itemProp in propSum)
                {
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;
                        var propValue = await GetReferenceSumValue(resutlDictionary, itemProp);
                        resutlDictionary[itemProp.Name] = propValue;
                    }
                }
            }

            var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                             x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                             x.GetCustomAttribute<ChildListAttribute>(false) != null).ToList();

            if (propsCollection.Count > 0)
            {
                foreach (var prop in propsCollection)
                {
                    var propName = prop.Name;
                    foreach (var item in tmpResutls)
                    {
                        var resutlDictionary = item as IDictionary<string, object>;

                        var defaultAll = propName + ".All";

                        var isAllField = fields.Any(x => x == defaultAll);

                        if (isAllField)
                        {
                            var data = await GetChildListObjectAsync(prop, item, false, null, tenantCode);

                            resutlDictionary[prop.Name] = data;
                        }
                        else
                        {
                            var childFields = fields.Where(x => x.Contains(prop.Name) && x != defaultAll).ToList();
                            if (childFields.Count > 0)
                            {
                                var childFieldsName = childFields.Select(x => x.Replace(propName + ".", "")).ToList();
                                var data = await GetChildListObjectAsync(prop, item, false, childFieldsName, tenantCode);

                                resutlDictionary[prop.Name] = data;
                            }
                        }
                    }
                }
            }

            return tmpResutls;

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<TEntity>> SearchPaging(List<SearchDTO> searchList, int pageSize, int pageIndex,
            SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY,
            List<string> fields = null, string tenantCode = null)
        {

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryMultipleDynamicAsync(rawQuery, tenantCode);
            var result = new ResultDTO<TEntity>();

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    var config = new MapperConfiguration(cfg => { });
                    var mapper = config.CreateMapper();

                    var mapObjects = mapper.Map<IList<dynamic>, IList<TEntity>>(tmpResutls);

                    result.Data = mapObjects;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<object>> SearchPagingFieldsAsync(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);
            var result = new ResultDTO<object>();

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    foreach (IDictionary<string, object> item in tmpResutls)
                    {
                        item.Remove("TotalRow");
                    }

                    result.Data = tmpResutls;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<object>> SearchPagingFieldsAsyncV2(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPagingV2(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);
            var countTotal = await CountAsync(searchList, dataLevel, companyCode, tenantCode);

            var result = new ResultDTO<object>();
            result.Total = countTotal;
            result.Data = tmpResutls;

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="initPages"></param>
        /// <param name="extendPages"></param>
        /// <param name="rowOffset"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<object>> SearchPagingFieldsAsyncV3(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, int initPages = 5, int extendPages = 2, int rowOffset = 0,
          string tenantCode = null)
        {
            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                        pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPagingV3(pageSize,
                        pageIndex, rowOffset, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);

            var result = new ResultDTO<object>
            {
                Total = tmpResutls.Count > 0 ? ((pageIndex - 1) * pageSize) + tmpResutls.Count : 0,
                Data = tmpResutls
            };

            if (tmpResutls.Count == pageSize)
            {
                int middleInitPage = (int)Math.Ceiling((double)initPages / 2);
                int pageSizeNext = pageIndex > middleInitPage
                                    ? ((extendPages - 1) * pageSize + 1)
                                    : ((initPages - 1) * pageSize + 1) - (pageIndex * pageSize);

                if (pageSizeNext > 0)
                {
                    int offset = (int)result.Total;
                    var countTotal = await CountLimit(searchList, pageSizeNext,
                                offset, fields, sortDTO, companyCode, dataLevel, tenantCode);

                    result.Total = result.Total + countTotal;
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<TEntity>> SearchIncludeChildrenPaging(List<SearchDTO> searchList, int pageSize,
            int pageIndex, SortDTO sortDTO = null, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null)
        {

            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                 pageIndex, tableName, refTblList, searchList, sortDTO);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize,
                 pageIndex, tableName, refTblList, searchList, sortDTO);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryMultipleDynamicAsync(rawQuery, tenantCode);
            var result = new ResultDTO<TEntity>();

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    var config = new MapperConfiguration(cfg => { });
                    var mapper = config.CreateMapper();

                    var mapObjects = mapper.Map<IList<dynamic>, IList<TEntity>>(tmpResutls);

                    var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                       x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)).ToList();

                    propsCollection.ForEach(prop =>
                    {
                        foreach (var item in mapObjects)
                        {
                            var data = GetChildListObject(prop, item, false, null, tenantCode);
                            prop.SetValue(item, data);
                        }
                    });

                    result.Data = mapObjects;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<ResultDTO<object>> SearchIncludeChildrenPagingFields(List<SearchDTO> searchList, int pageSize,
          int pageIndex, List<string> fields, SortDTO sortDTO = null, object companyCode = null,
          DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            if (!fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            fields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryPaging(pageSize,
                 pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryPaging(pageSize,
                 pageIndex, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                default:
                    break;
            }

            dynamic tmpResutls = await _repo.RawQueryAsync(rawQuery, tenantCode);
            var result = new ResultDTO<object>();

            if (tmpResutls != null)
            {
                var enumList = tmpResutls as IEnumerable<dynamic>;
                var firstRow = enumList.FirstOrDefault();

                if (firstRow != null)
                {
                    var firstDic = firstRow as IDictionary<string, object>;
                    var totalRow = firstDic["TotalRow"].ToString();

                    var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                       x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)).ToList();

                    if (propsCollection.Any())
                    {
                        foreach (var prop in propsCollection)
                        {
                            var propName = prop.Name;
                            foreach (var item in tmpResutls)
                            {
                                var resutlDictionary = item as IDictionary<string, object>;

                                var defaultAll = propName + ".All";

                                var isAllField = fields.Any(x => x == defaultAll);

                                if (isAllField)
                                {
                                    var data = await GetChildListObjectAsync(prop, item, false, null, tenantCode);

                                    resutlDictionary[prop.Name] = data;
                                }
                                else
                                {
                                    var childFields = fields.Where(x => x.Contains(prop.Name) && x != defaultAll).ToList();
                                    if (childFields.Count > 0)
                                    {
                                        var childFieldsName = childFields.Select(x => x.Replace(propName + ".", "")).ToList();
                                        var data = await GetChildListObjectAsync(prop, item, false, childFieldsName, tenantCode);

                                        resutlDictionary[prop.Name] = data;
                                    }
                                }

                                resutlDictionary.Remove("TotalRow");
                            }
                        }
                    }
                    else
                    {
                        foreach (IDictionary<string, object> item in tmpResutls)
                        {
                            item.Remove("TotalRow");
                        }
                    }

                    result.Data = tmpResutls;
                    result.Total = long.Parse(totalRow);
                }
                else
                {
                    result.Data = null;
                    result.Total = 0;
                }
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<long> CountAsync(List<SearchDTO> searchList,
           DataLevel dataLevel = DataLevel.COMPANY, object companyCode = null, string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:

                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryCount(tableName, searchList);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQueryFirstAsync(rawQuery, tenantCode);
            if (result != null)
            {
                var dict = result as IDictionary<string, object>;
                dict.TryGetValue("count", out var count);
                return (long)count;
            }

            return 0;
        }

        /// <summary>
        /// Count Limit
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<long> CountLimit(List<SearchDTO> searchList, int size, int offset, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(fields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:

                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawCountQueryPagingV3(size,
                        offset, tableName, refTblList, searchList, sortDTO, fields);
                    break;
                default:
                    break;
            }

            var result = await _repo.RawQueryFirstAsync(rawQuery, tenantCode);
            if (result != null)
            {
                var dict = result as IDictionary<string, object>;
                dict.TryGetValue("TotalRow", out var count);
                return (long)count;
            }
            return 0;
        }
        #endregion query data

        #region private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propInfo"></param>
        /// <returns></returns>
        public async Task<object> GetReferenceOneToOneValue(dynamic item, PropertyInfo propInfo)
        {
            OneToOneAttribute propOneToOne = propInfo.GetCustomAttribute<OneToOneAttribute>(false);

            var companyCode = PropertyUtilities.GetPropValueOfObject(item, TableColumnConst.COMPANY_CODE_COL);
            var originTableCol = propOneToOne.OriginColumn;
            var refCol = propOneToOne.RefColumn;
            var valCol = propOneToOne.ColVal;

            var originTableVal = PropertyUtilities.GetPropValueOfObject(item, originTableCol);

            var searchList = new List<SearchDTO>();
            searchList.Add(new SearchDTO() { SearchCondition = SearchCondition.Equal, SearchField = refCol, SearchValue = originTableVal });

            var refTypeService = typeof(IBaseService<,>).MakeGenericType(new Type[] { typeof(TDataContext), propOneToOne.ReferenceTableType });
            var baseServiceRef = _serviceProvider.GetRequiredService(refTypeService);
            var method = baseServiceRef.GetType().GetMethod("SearchSingleAsync");

            Task<object> resultTask = (Task<object>)method.Invoke(baseServiceRef, new object[] { searchList, new List<string> { valCol },
                null,companyCode, DataLevel.COMPANY, null});

            var result = await resultTask;
            var val = PropertyUtilities.GetPropValueOfObject(result, valCol);
            return val;
        }


        public async Task<object> GetReferenceOneToOneValue(IDictionary<string, object> item, PropertyInfo propInfo)
        {
            OneToOneAttribute propOneToOne = propInfo.GetCustomAttribute<OneToOneAttribute>(false);

            var companyCode = item[TableColumnConst.COMPANY_CODE_COL];
            var originTableCol = propOneToOne.OriginColumn;
            var refCol = propOneToOne.RefColumn;
            var valCol = propOneToOne.ColVal;

            var originTableVal = item[originTableCol];

            var searchList = new List<SearchDTO>();
            searchList.Add(new SearchDTO() { SearchCondition = SearchCondition.Equal, SearchField = refCol, SearchValue = originTableVal });

            var refTypeService = typeof(IBaseService<,>).MakeGenericType(new Type[] { typeof(TDataContext), propOneToOne.ReferenceTableType });
            var baseServiceRef = _serviceProvider.GetRequiredService(refTypeService);
            var method = baseServiceRef.GetType().GetMethod("SearchSingleAsync");

            Task<object> resultTask = (Task<object>)method.Invoke(baseServiceRef, new object[] { searchList, new List<string> { valCol },
                null,companyCode, DataLevel.COMPANY, null});

            var result = await resultTask;
            var val = PropertyUtilities.GetPropValueOfObject(result, valCol);
            return val;
        }

        public async Task<object> GetReferenceSumValue(IDictionary<string, object> item, PropertyInfo propInfo)
        {
            SumAttribute sumAttribute = propInfo.GetCustomAttribute<SumAttribute>(false);

            var companyCode = item[TableColumnConst.COMPANY_CODE_COL];
            var originTableCol = sumAttribute.OriginColumn;
            var refCol = sumAttribute.RefColumn;
            var valCol = sumAttribute.ColVal;

            var originTableVal = item[originTableCol];

            var searchList = new List<SearchDTO>();
            searchList.Add(new SearchDTO() { SearchCondition = SearchCondition.Equal, SearchField = refCol, SearchValue = originTableVal });

            var refTypeService = typeof(IBaseService<,>).MakeGenericType(new Type[] { typeof(TDataContext), sumAttribute.ReferenceTableType });
            var baseServiceRef = _serviceProvider.GetRequiredService(refTypeService);
            var method = baseServiceRef.GetType().GetMethods().First(x => x.Name == "SearchFieldsAysnc" && !x.IsGenericMethod);

            Task<object> resultTask = (Task<object>)method.Invoke(baseServiceRef, new object[] { searchList, new List<string> { valCol },
                null,companyCode, DataLevel.COMPANY, null, 0});

            dynamic result = await resultTask;
            var sum = 0.0;

            if (result != null)
            {
                foreach (IDictionary<string, object> subItem in result)
                {
                    var val = subItem[valCol];
                    if (val != null)
                    {
                        var outPut = 0.0;
                        if (double.TryParse(val.ToString(), out outPut))
                        {
                            sum += outPut;
                        }
                    }
                }
            }

            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propInfo"></param>
        /// <returns></returns>
        public async Task<object> GetReferenceSumValue(dynamic item, PropertyInfo propInfo)
        {
            SumAttribute propOneToOne = propInfo.GetCustomAttribute<SumAttribute>(false);

            var companyCode = PropertyUtilities.GetPropValueOfObject(item, TableColumnConst.COMPANY_CODE_COL);
            var originTableCol = propOneToOne.OriginColumn;
            var refCol = propOneToOne.RefColumn;
            var valCol = propOneToOne.ColVal;

            var originTableVal = PropertyUtilities.GetPropValueOfObject(item, originTableCol);

            var searchList = new List<SearchDTO>();
            searchList.Add(new SearchDTO() { SearchCondition = SearchCondition.Equal, SearchField = refCol, SearchValue = originTableVal });

            var refTypeService = typeof(IBaseService<,>).MakeGenericType(new Type[] { typeof(TDataContext), propOneToOne.ReferenceTableType });
            var baseServiceRef = _serviceProvider.GetRequiredService(refTypeService);
            var method = baseServiceRef.GetType().GetMethods().First(x => x.Name == "SearchFieldsAysnc" && !x.IsGenericMethod);

            Task<object> resultTask = (Task<object>)method.Invoke(baseServiceRef, new object[] { searchList, new List<string> { valCol },
                null,companyCode, DataLevel.COMPANY, null, 0});

            dynamic result = await resultTask;
            var sum = 0.0;

            if (result != null)
            {
                foreach (IDictionary<string, object> subItem in result)
                {
                    var val = subItem[valCol];
                    if (val != null)
                    {
                        var outPut = 0.0;
                        if (double.TryParse(val.ToString(), out outPut))
                        {
                            sum += outPut;
                        }
                    }
                }
            }

            return sum;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public dynamic GetChildListObject(PropertyInfo prop, dynamic parent,
            bool includeParent = false, List<string> fields = null, string tenantCode = null)
        {
            //get property with attribute's name is ChildListAttribute
            var foreignkeyAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

            if (foreignkeyAttribute != null)
            {
                bool manyToManyRelation = foreignkeyAttribute.ManyToManyRelation;
                var foreignKeyTableName = manyToManyRelation ? foreignkeyAttribute.CombineKey : foreignkeyAttribute.ForeignKeyCode;
                OptionJoin optionJoin = foreignkeyAttribute.OptionJoin as OptionJoin;

                Type childTypeProp = prop.PropertyType;
                var genericType = childTypeProp.GenericTypeArguments[0];

                Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                var pkValue = parent.Code;


                List<SearchDTO> conditions = new List<SearchDTO>();
                conditions.Add(new SearchDTO()
                {
                    SearchField = foreignKeyTableName,
                    SearchValue = pkValue,
                    SearchCondition = SearchCondition.Equal
                });

                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                    SearchValue = ModifiedType.Delete.ToString(),
                    SearchCondition = SearchCondition.NotEqual
                });

                if (optionJoin != null)
                {
                    var fieldFromVal = PropertyUtilities.GetPropValueOfObject(parent, optionJoin.From);
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = optionJoin.To,
                        SearchValue = fieldFromVal,
                        SearchCondition = SearchCondition.Equal
                    });
                }

                var tableConfig = listTableConfig.FirstOrDefault(x => x.TableName == genericType.Name);

                if (tableConfig != null)
                {
                    if (tableConfig.TableType != "All")
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = parent.CompanyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                }
                else
                {
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = parent.CompanyCode,
                        SearchCondition = SearchCondition.Equal
                    });
                }

                var rawQuery = "";
                var tableChildName = Utilities.GetTableName(_repo.DataContext, childClassType);

                if (!includeParent)
                {
                    PropertyInfo[] propsInfo = childClassType.GetProperties();
                    var propsParents = propsInfo.Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

                    List<ReferenceTable> refTblList = new List<ReferenceTable>();

                    foreach (var propParent in propsParents)
                    {
                        //get property with attribute's name is ChildListAttribute
                        var fkeyProp = propParent.GetCustomAttribute<ParentAttribute>(false);
                        if (fkeyProp.InDetail)
                        {
                            var foreignKeyColName = fkeyProp.ParentColName;

                            var parentTableName = fkeyProp.TableName;

                            var refCol = fkeyProp.RefColName;

                            if (parentTableName != null)
                            {
                                var tbConfig = listTableConfig.FirstOrDefault(x => x.TableName == parentTableName);

                                if (tbConfig != null)
                                {
                                    refTblList.Add(new ReferenceTable()
                                    {
                                        AliasName = propParent.Name,
                                        ColumnName = refCol,
                                        TableName = parentTableName,
                                        ForeignKeyCol = foreignKeyColName,
                                        FilterType = tbConfig.TableType
                                    });
                                }
                                else
                                {
                                    refTblList.Add(new ReferenceTable()
                                    {
                                        AliasName = propParent.Name,
                                        ColumnName = refCol,
                                        TableName = parentTableName,
                                        ForeignKeyCol = foreignKeyColName,
                                        FilterType = ""
                                    });
                                }
                            }
                        }
                    }

                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:

                            rawQuery = SqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    PropertyInfo[] propsInfo = childClassType.GetProperties();
                    var propsParents = propsInfo.Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

                    List<ReferenceTable> refTblList = new List<ReferenceTable>();

                    foreach (var propParent in propsParents)
                    {
                        //get property with attribute's name is ChildListAttribute
                        var fkeyProp = propParent.GetCustomAttribute<ParentAttribute>(false);
                        var foreignKeyColName = fkeyProp.ParentColName;

                        var parentTableName = fkeyProp.TableName;

                        var refCol = fkeyProp.RefColName;

                        if (parentTableName != null)
                        {
                            var tbConfig = listTableConfig.FirstOrDefault(x => x.TableName == parentTableName);

                            if (tbConfig != null)
                            {
                                refTblList.Add(new ReferenceTable()
                                {
                                    AliasName = propParent.Name,
                                    ColumnName = refCol,
                                    TableName = parentTableName,
                                    ForeignKeyCol = foreignKeyColName,
                                    FilterType = tbConfig.TableType
                                });
                            }
                            else
                            {
                                refTblList.Add(new ReferenceTable()
                                {
                                    AliasName = propParent.Name,
                                    ColumnName = refCol,
                                    TableName = parentTableName,
                                    ForeignKeyCol = foreignKeyColName,
                                    FilterType = ""
                                });
                            }
                        }
                    }

                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            rawQuery = SqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);
                            break;
                        default:
                            break;
                    }
                }

                if (fields == null)
                {
                    var method = _repo.GetType().GetMethod("RawQueryMultiple", BindingFlags.Public |
                        BindingFlags.Instance).MakeGenericMethod(childClassType);
                    dynamic result = method.Invoke(_repo, new object[] { rawQuery, tenantCode });

                    return result;
                }
                else
                {
                    var method = _repo.GetType().GetMethod("RawQueryAsync", BindingFlags.Public |
                        BindingFlags.Instance);
                    dynamic result = method.Invoke(_repo, new object[] { rawQuery, tenantCode });

                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="parent"></param>
        /// <param name="includeParent"></param>
        /// <param name="fields"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<object> GetChildListObjectAsync(PropertyInfo prop, dynamic parent,
          bool includeParent = false, List<string> fields = null, string tenantCode = null)
        {
            //get property with attribute's name is ChildListAttribute
            var foreignkeyAttribute = prop.GetCustomAttribute<ChildListAttribute>(false);

            if (foreignkeyAttribute != null)
            {
                bool manyToManyRelation = foreignkeyAttribute.ManyToManyRelation;
                var foreignKeyTableName = manyToManyRelation ? foreignkeyAttribute.CombineKey : foreignkeyAttribute.ForeignKeyCode;
                OptionJoin optionJoin = foreignkeyAttribute.OptionJoin as OptionJoin;

                Type childTypeProp = prop.PropertyType;
                var genericType = childTypeProp.GenericTypeArguments[0];

                Type childClassType = Type.GetType(genericType.FullName + "," + prop.DeclaringType.Assembly.FullName);

                var pkValue = parent.Code;


                List<SearchDTO> conditions = new List<SearchDTO>();
                conditions.Add(new SearchDTO()
                {
                    SearchField = foreignKeyTableName,
                    SearchValue = pkValue,
                    SearchCondition = SearchCondition.Equal
                });

                conditions.Add(new SearchDTO()
                {
                    SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                    SearchValue = ModifiedType.Delete.ToString(),
                    SearchCondition = SearchCondition.NotEqual
                });

                if (optionJoin != null)
                {
                    var fieldFromVal = PropertyUtilities.GetPropValueOfObject(parent, optionJoin.From);
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = optionJoin.To,
                        SearchValue = fieldFromVal,
                        SearchCondition = SearchCondition.Equal
                    });
                }

                var tableConfig = listTableConfig.FirstOrDefault(x => x.TableName == genericType.Name);

                if (tableConfig != null)
                {
                    if (tableConfig.TableType != "All")
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = parent.CompanyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                }
                else
                {
                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = parent.CompanyCode,
                        SearchCondition = SearchCondition.Equal
                    });
                }

                var rawQuery = "";
                var tableChildName = Utilities.GetTableName(_repo.DataContext, childClassType);
                var validateFields = PropertyUtilities.CheckValidateFields(_repo.DataContext, fields, childClassType);

                if (!includeParent)
                {
                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:

                            rawQuery = SqlUtilities.BuildRawQueryJoin(tableChildName, null, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableChildName, null, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, fields);

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    PropertyInfo[] propsInfo = childClassType.GetProperties();
                    var propsParents = propsInfo.Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

                    List<ReferenceTable> refTblList = new List<ReferenceTable>();

                    foreach (var propParent in propsParents)
                    {
                        //get property with attribute's name is ChildListAttribute
                        var fkeyProp = propParent.GetCustomAttribute<ParentAttribute>(false);
                        var foreignKeyColName = fkeyProp.ParentColName;

                        var parentTableName = fkeyProp.TableName;

                        var refCol = fkeyProp.RefColName;

                        if (parentTableName != null)
                        {
                            var tbConfig = listTableConfig.FirstOrDefault(x => x.TableName == parentTableName);

                            if (tbConfig != null)
                            {
                                refTblList.Add(new ReferenceTable()
                                {
                                    AliasName = propParent.Name,
                                    ColumnName = refCol,
                                    TableName = parentTableName,
                                    ForeignKeyCol = foreignKeyColName,
                                    FilterType = tbConfig.TableType
                                });
                            }
                            else
                            {
                                refTblList.Add(new ReferenceTable()
                                {
                                    AliasName = propParent.Name,
                                    ColumnName = refCol,
                                    TableName = parentTableName,
                                    ForeignKeyCol = foreignKeyColName,
                                    FilterType = ""
                                });
                            }
                        }
                    }

                    switch (_databaseProvider)
                    {
                        case DatabaseProvider.MSSQL:
                            rawQuery = SqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions,
                                 new SortDTO
                                 {
                                     Sort = "asc",
                                     SortBy = foreignKeyTableName
                                 }, validateFields);
                            break;
                        case DatabaseProvider.POSTGRESQL:
                            rawQuery = PostgresSqlUtilities.BuildRawQueryJoin(tableChildName, refTblList, conditions, new SortDTO
                            {
                                Sort = "asc",
                                SortBy = foreignKeyTableName
                            }, validateFields);
                            break;
                        default:
                            break;
                    }
                }

                if (fields == null)
                {
                    var method = _repo.GetType().GetMethod("RawQueryMultiple", BindingFlags.Public |
                        BindingFlags.Instance).MakeGenericMethod(childClassType);
                    dynamic result = method.Invoke(_repo, new object[] { rawQuery, tenantCode });

                    return result;
                }
                else
                {
                    var result = await _repo.RawQueryAsync(rawQuery, tenantCode);

                    return result;
                }

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedColumns"></param>
        /// <returns></returns>
        public List<ReferenceTable> GetReferenceColumns(List<string> selectedColumns)
        {
            List<ReferenceTable> refTblList = new List<ReferenceTable>();

            PropertyInfo[] propsInfo = typeof(TEntity).GetProperties();

            var propsParents = propsInfo.Where(x => x.GetCustomAttribute(typeof(ParentAttribute)) != null);

            if (selectedColumns != null)
            {
                propsParents = propsParents.Where(x => selectedColumns.Any(p => p.ToString() == x.Name)).ToList();
            }

            foreach (var prop in propsParents)
            {
                //get property with attribute's name is ChildListAttribute
                var foreignkeyProp = (ParentAttribute)prop.GetCustomAttribute(typeof(ParentAttribute), false);

                var foreignKeyColName = foreignkeyProp.ParentColName;
                var parentTableName = foreignkeyProp.TableName;
                var refCol = foreignkeyProp.RefColName;
                object[] optionJoin = foreignkeyProp.OptionJoin as object[];

                if (!string.IsNullOrEmpty(parentTableName))
                {
                    var tableConfig = listTableConfig.FirstOrDefault(x => x.TableName == parentTableName);

                    var optJoins = optionJoin != null ? optionJoin.Cast<string>()
                             .ToArray() : null;

                    if (tableConfig != null)
                    {
                        refTblList.Add(new ReferenceTable()
                        {
                            AliasName = prop.Name,
                            ColumnName = refCol,
                            TableName = parentTableName,
                            ForeignKeyCol = foreignKeyColName,
                            FilterType = tableConfig.TableType,
                            OptionJoin = optJoins
                        });
                    }
                    else
                    {
                        refTblList.Add(new ReferenceTable()
                        {
                            AliasName = prop.Name,
                            ColumnName = refCol,
                            TableName = parentTableName,
                            ForeignKeyCol = foreignKeyColName,
                            FilterType = "",
                            OptionJoin = optJoins
                        });
                    }
                }
            }

            return refTblList;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="distributedConfig"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        private async Task<SearchDTO> GetDistributedConditionAsync(DistributedConfig distributedConfig,
            object companyCode, string tenantCode = null)
        {
            SearchDTO condition = null;

            var tbCompanyName = _distributedCompany.TableCompany;

            var searchChildCompany = new List<SearchDTO>();
            searchChildCompany.Add(new SearchDTO()
            {
                SearchCondition = SearchCondition.Equal,
                SearchField = TableColumnConst.PARENT_CODE,
                SearchValue = companyCode
            });

            var query = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:

                    query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                case DatabaseProvider.POSTGRESQL:

                    query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                default:
                    break;
            }

            dynamic childCompanies = await _repo.RawQueryMultipleDynamicAsync(query, tenantCode);

            List<object> companyCodes = new List<object>();

            switch (distributedConfig.DistributedType)
            {
                case DistributedType.Single:
                    if (childCompanies.Count > 0)
                    {
                        foreach (var childrenCompany in childCompanies)
                        {
                            companyCodes.Add(childrenCompany.Code);
                        }

                        condition = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.DISTRIBUTED_COL,
                            SearchValue = JArray.FromObject(companyCodes),
                            CombineCondition = "OR"
                        };
                    }
                    else
                    {
                        condition = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.DISTRIBUTED_COL,
                            SearchValue = companyCode,
                            CombineCondition = "OR"
                        };
                    }
                    break;

                case DistributedType.Multiple:
                    //is company
                    if (childCompanies.Count > 0)
                    {
                        foreach (dynamic childrenCompany in childCompanies)
                        {
                            companyCodes.Add(childrenCompany.Code);
                        }

                        var conditionCompany = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = distributedConfig.RefCompanyColName,
                            SearchValue = JArray.FromObject(companyCodes),
                        };

                        conditionCompany.GroupID = 1;

                        var searchs = new List<SearchDTO>() { conditionCompany };

                        searchs.Add(new SearchDTO()
                        {
                            CombineCondition = "OR",
                            GroupID = 1,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal,
                            SearchField = distributedConfig.RefCompanyColName
                        });

                        var refQuery = "";

                        switch (_databaseProvider)
                        {
                            case DatabaseProvider.MSSQL:
                                refQuery = SqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                                   searchs, new SortDTO()
                                   { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            case DatabaseProvider.POSTGRESQL:
                                refQuery = PostgresSqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                                   searchs, new SortDTO()
                                   { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            default:
                                break;
                        }

                        List<dynamic> listItems = await _repo.RawQueryMultipleDynamicAsync(refQuery, tenantCode);

                        var arrayItems = (JArray.FromObject(listItems)).Select(x => x[distributedConfig.RefColItemName]).ToList();

                        if (listItems.Count > 0)
                        {
                            condition = new SearchDTO()
                            {
                                SearchCondition = SearchCondition.Equal,
                                SearchField = TableColumnConst.CODE_COL,
                                SearchValue = JArray.FromObject(arrayItems),
                                CombineCondition = "AND"
                            };
                        }
                    }
                    //is station
                    else
                    {
                        var conditionCompany = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = distributedConfig.RefCompanyColName,
                            SearchValue = companyCode,
                        };

                        var refQuery = "";

                        switch (_databaseProvider)
                        {
                            case DatabaseProvider.MSSQL:
                                refQuery = SqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                                    new List<SearchDTO>() { conditionCompany }, new SortDTO()
                                    { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            case DatabaseProvider.POSTGRESQL:
                                refQuery = PostgresSqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                                   new List<SearchDTO>() { conditionCompany }, new SortDTO()
                                   { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            default:
                                break;
                        }

                        List<dynamic> listItems = await _repo.RawQueryMultipleDynamicAsync(refQuery, tenantCode);

                        var arrayItems = (JArray.FromObject(listItems)).Select(x => x[distributedConfig.RefColItemName]).ToList();

                        if (listItems.Count > 0)
                        {
                            condition = new SearchDTO()
                            {
                                SearchCondition = SearchCondition.Equal,
                                SearchField = TableColumnConst.CODE_COL,
                                SearchValue = JArray.FromObject(arrayItems),
                                CombineCondition = "AND"
                            };
                        }
                    }
                    break;

                default:
                    break;
            }

            return condition;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="distributedConfig"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        private SearchDTO GetDistributedCondition(DistributedConfig distributedConfig, object companyCode, string tenantCode)
        {
            SearchDTO condition = null;

            var tbCompanyName = _distributedCompany.TableCompany;

            var searchChildCompany = new List<SearchDTO>();
            searchChildCompany.Add(new SearchDTO()
            {
                SearchCondition = SearchCondition.Equal,
                SearchField = TableColumnConst.PARENT_CODE,
                SearchValue = companyCode
            });

            var query = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                default:
                    break;
            }

            dynamic childCompanies = _repo.RawQueryMultipleDynamic(query, tenantCode);

            List<object> companyCodes = new List<object>();

            switch (distributedConfig.DistributedType)
            {
                case DistributedType.Single:
                    if (childCompanies.Count > 0)
                    {
                        foreach (var childrenCompany in childCompanies)
                        {
                            companyCodes.Add(childrenCompany.Code);
                        }

                        condition = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.DISTRIBUTED_COL,
                            SearchValue = JArray.FromObject(companyCodes),
                            CombineCondition = "OR"
                        };
                    }
                    else
                    {
                        condition = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.DISTRIBUTED_COL,
                            SearchValue = companyCode,
                            CombineCondition = "OR"
                        };
                    }
                    break;

                case DistributedType.Multiple:
                    //is company
                    if (childCompanies.Count > 0)
                    {
                        foreach (dynamic childrenCompany in childCompanies)
                        {
                            companyCodes.Add(childrenCompany.Code);
                        }

                        var conditionCompany = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = distributedConfig.RefCompanyColName,
                            SearchValue = JArray.FromObject(companyCodes),
                        };

                        var refQuery = "";

                        switch (_databaseProvider)
                        {
                            case DatabaseProvider.MSSQL:
                                refQuery = SqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                            new List<SearchDTO>() { conditionCompany }, new SortDTO()
                            { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            case DatabaseProvider.POSTGRESQL:
                                refQuery = PostgresSqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                                    new List<SearchDTO>() { conditionCompany }, new SortDTO()
                                    { Sort = "asc", SortBy = TableColumnConst.COMPANY_CODE_COL }, new List<string> { distributedConfig.RefColItemName });
                                break;
                            default:
                                break;
                        }

                        List<dynamic> listItems = _repo.RawQueryMultipleDynamic(refQuery, tenantCode);

                        var arrayItems = (JArray.FromObject(listItems)).Select(x => x[distributedConfig.RefColItemName]).ToList();

                        if (listItems.Count > 0)
                        {
                            condition = new SearchDTO()
                            {
                                SearchCondition = SearchCondition.Equal,
                                SearchField = TableColumnConst.CODE_COL,
                                SearchValue = JArray.FromObject(arrayItems),
                                CombineCondition = "OR"
                            };
                        }
                    }
                    //is station
                    else
                    {
                        var conditionCompany = new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = distributedConfig.RefCompanyColName,
                            SearchValue = companyCode,
                        };

                        var refQuery = "";

                        switch (_databaseProvider)
                        {
                            case DatabaseProvider.MSSQL:
                                refQuery = SqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                           new List<SearchDTO>() { conditionCompany }, null, new List<string> { distributedConfig.RefColItemName });
                                break;
                            case DatabaseProvider.POSTGRESQL:
                                refQuery = PostgresSqlUtilities.BuildRawQuery(distributedConfig.TableDistributedCompany,
                           new List<SearchDTO>() { conditionCompany }, null, new List<string> { distributedConfig.RefColItemName });
                                break;
                            default:
                                break;
                        }

                        List<dynamic> listItems = _repo.RawQueryMultipleDynamic(refQuery, tenantCode);

                        var arrayItems = (JArray.FromObject(listItems)).Select(x => x[distributedConfig.RefColItemName]).ToList();

                        if (listItems.Count > 0)
                        {
                            condition = new SearchDTO()
                            {
                                SearchCondition = SearchCondition.Equal,
                                SearchField = TableColumnConst.CODE_COL,
                                SearchValue = JArray.FromObject(arrayItems),
                                CombineCondition = "OR"
                            };
                        }
                    }
                    break;

                default:
                    break;
            }

            return condition;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <returns></returns>
        private async Task<List<SearchDTO>> BuildSearchConditionsAsync(string tableName, object companyCode, DataLevel dataLevel,
            string tenantCode = null)
        {
            List<SearchDTO> conditions = new List<SearchDTO>();
            var haveDistributeData = false;

            DistributedConfig distributedConfig = null;
            //get distributed condition
            if (_distributedCompany.DistributedConfigs != null)
            {
                var distributedTable = _distributedCompany.DistributedConfigs == null ? null : _distributedCompany.DistributedConfigs.
                    FirstOrDefault(x => x.TableName == tableName);

                if (distributedTable != null)
                {
                    distributedConfig = distributedTable;
                }

                if (distributedConfig != null)
                {
                    //Kiếm dòng dữ liệu phân bổ
                    var condition = await GetDistributedConditionAsync(distributedConfig, companyCode, tenantCode);

                    if (condition != null)
                    {
                        condition.GroupID = 1;
                        conditions.Add(condition);
                        conditions.Add(new SearchDTO()
                        {
                            GroupID = 1,
                            CombineCondition = "OR",
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode
                        });

                        haveDistributeData = true;
                    }
                }
            }

            if (distributedConfig == null)
            {
                if (dataLevel == DataLevel.COMPANY)
                {
                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.EqualExact
                        });
                    }
                }
                else if (dataLevel == DataLevel.COMPANYINCLUDECHILD && companyCode != null)
                {
                    var childCompanies = await GetChildCompanies(_distributedCompany.TableCompany,
                        companyCode.ToString(), _distributedCompany.ParentColumnName, new List<string>());

                    if (childCompanies.Count > 0)
                    {
                        childCompanies.Add(companyCode.ToString());

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = JArray.FromObject(childCompanies),
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                }
            }
            else
            {
                var tbCompanyName = _distributedCompany.TableCompany;

                var searchChildCompany = new List<SearchDTO>();
                searchChildCompany.Add(new SearchDTO()
                {
                    SearchCondition = SearchCondition.Equal,
                    SearchField = TableColumnConst.PARENT_CODE,
                    SearchValue = companyCode
                });
                //find children of current company
                var query = "";

                switch (_databaseProvider)
                {
                    case DatabaseProvider.MSSQL:
                        query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    case DatabaseProvider.POSTGRESQL:
                        query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    default:
                        break;
                }

                dynamic childCompanies = await _repo.RawQueryMultipleDynamicAsync(query, tenantCode);

                List<object> companyCodes = new List<object>();

                searchChildCompany.Clear();
                searchChildCompany.Add(new SearchDTO()
                {
                    SearchCondition = SearchCondition.Equal,
                    SearchField = TableColumnConst.CODE_COL,
                    SearchValue = companyCode
                });
                //get current company
                var comSql = "";

                switch (_databaseProvider)
                {
                    case DatabaseProvider.MSSQL:
                        comSql = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    case DatabaseProvider.POSTGRESQL:
                        comSql = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    default:
                        break;
                }

                dynamic currentCompany = await _repo.RawQueryDynamicSingle(comSql, tenantCode);

                if (currentCompany != null && currentCompany.ParentCode != null)
                {
                    //get all parent of current company
                    List<string> parentCompanies = await GetParentCompanies(tbCompanyName,
                        currentCompany.ParentCode, new List<string>(), tenantCode);

                    if (parentCompanies.Count > 0)
                    {
                        foreach (var item in parentCompanies)
                        {
                            companyCodes.Add(item);
                        }
                    }
                }

                if (childCompanies.Count > 0)
                {
                    foreach (var item in childCompanies)
                    {
                        companyCodes.Add(item.Code);
                    }

                    companyCodes.Add(companyCode);

                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = JArray.FromObject(companyCodes),
                        SearchCondition = SearchCondition.Equal
                    });
                }
                else
                {
                    if (haveDistributeData)
                    {
                        companyCodes.Add(companyCode);

                        conditions.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = JArray.FromObject(companyCodes)
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode
                        });
                    }

                }
            }

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                SearchValue = ModifiedType.Delete.ToString(),
                SearchCondition = SearchCondition.NotEqual
            });

            return conditions;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <returns></returns>
        private List<SearchDTO> BuildSearchConditions(string tableName, object companyCode, DataLevel dataLevel, string tenantCode = null)
        {
            List<SearchDTO> conditions = new List<SearchDTO>();

            DistributedConfig distributedConfig = null;

            if (_distributedCompany != null)
            {
                var distributedTable = _distributedCompany.DistributedConfigs == null ? null :
                    _distributedCompany.DistributedConfigs.FirstOrDefault(x => x.TableName == tableName);

                if (distributedTable != null)
                {
                    distributedConfig = distributedTable;
                }

                if (distributedConfig != null)
                {
                    var condition = GetDistributedCondition(distributedConfig, companyCode, tenantCode);

                    if (condition != null)
                    {
                        condition.GroupID = 1;
                        conditions.Add(condition);
                        conditions.Add(new SearchDTO()
                        {
                            GroupID = 1,
                            CombineCondition = "OR",
                            SearchCondition = SearchCondition.Equal,
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode
                        });
                    }
                }
            }

            if (distributedConfig == null)
            {
                if (dataLevel == DataLevel.COMPANY)
                {
                    if (companyCode != null)
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                }
                else if (dataLevel == DataLevel.COMPANYINCLUDECHILD && companyCode != null)
                {
                    var childCompanies = GetChildCompanies(_distributedCompany.TableCompany,
                        companyCode.ToString(), _distributedCompany.ParentColumnName, new List<string>(), tenantCode).Result;

                    if (childCompanies.Count > 0)
                    {
                        childCompanies.Add(companyCode.ToString());

                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = JArray.FromObject(childCompanies),
                            SearchCondition = SearchCondition.Equal
                        });
                    }
                    else
                    {
                        conditions.Add(new SearchDTO()
                        {
                            SearchField = TableColumnConst.COMPANY_CODE_COL,
                            SearchValue = companyCode,
                            SearchCondition = SearchCondition.Equal
                        });
                    }

                }
            }
            else
            {
                var tbCompanyName = _distributedCompany.TableCompany;

                var searchChildCompany = new List<SearchDTO>();
                searchChildCompany.Add(new SearchDTO()
                {
                    SearchCondition = SearchCondition.Equal,
                    SearchField = TableColumnConst.PARENT_CODE,
                    SearchValue = companyCode
                });
                searchChildCompany.Add(new SearchDTO()
                {
                    SearchCondition = SearchCondition.NotEqual,
                    SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                    SearchValue = ModifiedType.Delete.ToString()
                });
                //find children of current company
                var query = "";

                switch (_databaseProvider)
                {
                    case DatabaseProvider.MSSQL:
                        query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    case DatabaseProvider.POSTGRESQL:
                        query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    default:
                        break;
                }

                dynamic childCompanies = _repo.RawQueryMultipleDynamicAsync(query, tenantCode).Result;

                List<object> companyCodes = new List<object>();

                searchChildCompany.Clear();
                searchChildCompany.Add(new SearchDTO()
                {
                    SearchCondition = SearchCondition.Equal,
                    SearchField = TableColumnConst.CODE_COL,
                    SearchValue = companyCode
                });

                //get current company
                var comSql = "";

                switch (_databaseProvider)
                {
                    case DatabaseProvider.MSSQL:
                        comSql = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    case DatabaseProvider.POSTGRESQL:
                        comSql = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                        break;
                    default:
                        break;
                }

                dynamic currentCompany = _repo.RawQueryDynamicSingle(comSql).Result;

                //get all parent of current company
                if (currentCompany != null && currentCompany.ParentCode != null)
                {
                    List<string> parentCompanies = GetParentCompanies(tbCompanyName,
                        currentCompany.ParentCode, new List<string>(), tenantCode).Result;

                    if (parentCompanies.Count > 0)
                    {
                        foreach (var item in parentCompanies)
                        {
                            companyCodes.Add(item);
                        }
                    }
                }

                if (childCompanies.Count > 0)
                {
                    foreach (var item in childCompanies)
                    {
                        companyCodes.Add(item.Code);
                    }

                    companyCodes.Add(companyCode);

                    conditions.Add(new SearchDTO()
                    {
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = JArray.FromObject(companyCodes),
                        SearchCondition = SearchCondition.Equal
                    });
                }
                else
                {
                    companyCodes.Add(companyCode);

                    conditions.Add(new SearchDTO()
                    {
                        SearchCondition = SearchCondition.Equal,
                        SearchField = TableColumnConst.COMPANY_CODE_COL,
                        SearchValue = JArray.FromObject(companyCodes)
                    });
                }
            }

            conditions.Add(new SearchDTO()
            {
                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                SearchValue = ModifiedType.Delete.ToString(),
                SearchCondition = SearchCondition.NotEqual
            });

            return conditions;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tbCompanyName"></param>
        /// <param name="parentCompanyCode"></param>
        /// <param name="companies"></param>
        /// <returns></returns>
        private async Task<List<string>> GetParentCompanies(string tbCompanyName, string parentCompanyCode,
            List<string> companies, string tenantCode = null)
        {

            var searchChildCompany = new List<SearchDTO>();
            searchChildCompany.Add(new SearchDTO()
            {
                SearchCondition = SearchCondition.Equal,
                SearchField = TableColumnConst.CODE_COL,
                SearchValue = parentCompanyCode
            });

            var query = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                default:
                    break;
            }

            dynamic parentCompany = await _repo.RawQueryDynamicSingle(query, tenantCode);

            if (parentCompany == null)
            {
                return companies;
            }
            else
            {
                companies.Add(parentCompany.Code);
                if (parentCompany.ParentCode != null)
                {
                    companies = await GetParentCompanies(tbCompanyName, parentCompany.ParentCode, companies, tenantCode);
                }
            }


            return companies;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tbCompanyName"></param>
        /// <param name="parentCode"></param>
        /// <param name="tbParentColumn"></param>
        /// <param name="companies"></param>
        /// <returns></returns>
        private async Task<List<string>> GetChildCompanies(string tbCompanyName, string parentCode, string tbParentColumn,
            List<string> companies, string tenantCode = null)
        {
            var searchChildCompany = new List<SearchDTO>();
            searchChildCompany.Add(new SearchDTO()
            {
                SearchCondition = SearchCondition.Equal,
                SearchField = tbParentColumn,
                SearchValue = parentCode
            });

            searchChildCompany.Add(new SearchDTO()
            {
                SearchCondition = SearchCondition.NotEqual,
                SearchField = TableColumnConst.MODIFIED_TYPE_COL,
                SearchValue = ModifiedType.Delete.ToString()
            });

            var query = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    query = SqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    query = PostgresSqlUtilities.BuildRawQuery(tbCompanyName, searchChildCompany, null);
                    break;
                default:
                    break;
            }

            var childCompanies = await _repo.RawQueryMultipleDynamicAsync(query, tenantCode);

            if (childCompanies.Count > 0)
            {
                foreach (var item in childCompanies)
                {
                    companies.Add(item.Code);
                    var subChildCompanies = await GetChildCompanies(tbCompanyName, item.Code, tbParentColumn, companies, tenantCode);
                    companies = subChildCompanies;
                }
            }

            return companies;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<object> SearchSingleAsync(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null)
        {
            var conditions = await BuildSearchConditionsAsync(tableName, companyCode, dataLevel, tenantCode);

            if (fields != null && !fields.Contains(TableColumnConst.CODE_COL))
            {
                fields.Add(TableColumnConst.CODE_COL);
            }

            var rootFields = fields;

            if (fields != null)
            {
                rootFields = PropertyUtilities.CheckValidateFields<TEntity>(_repo.DataContext, fields);
            }

            searchList.AddRange(conditions);

            // Distinct search list
            searchList = searchList.GroupBy(x => new
            {
                x.SearchField,
                x.SearchCondition,
                x.SearchValue,
                x.CombineCondition,
                x.GroupID
            }).Select(g => g.First()).ToList();

            List<ReferenceTable> refTblList = GetReferenceColumns(rootFields);

            string rawQuery = "";

            switch (_databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    rawQuery = SqlUtilities.BuildRawQueryJoinLimit(tableName, refTblList, searchList, sortDTO, rootFields);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    rawQuery = PostgresSqlUtilities.BuildRawQueryJoinLimit(tableName, refTblList, searchList, sortDTO, rootFields);
                    break;
                default:
                    break;
            }

            var item = await _repo.RawQuerySingleAsync(rawQuery, tenantCode);

            var propsCollection = typeof(TEntity).GetProperties().Where(x => x.PropertyType.IsGenericType &&
                             x.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                             x.GetCustomAttribute<ChildListAttribute>(false) != null).ToList();

            if (propsCollection.Count > 0)
            {
                foreach (var prop in propsCollection)
                {
                    var propName = prop.Name;

                    var resutlDictionary = item as IDictionary<string, object>;

                    var defaultAll = propName + ".All";

                    var isAllField = fields.Any(x => x == defaultAll);

                    if (isAllField)
                    {
                        var data = await GetChildListObjectAsync(prop, item, false, null, tenantCode);

                        resutlDictionary[prop.Name] = data;
                    }
                    else
                    {
                        var childFields = fields.Where(x => x.Contains(prop.Name) && x != defaultAll).ToList();
                        if (childFields.Count > 0)
                        {
                            var childFieldsName = childFields.Select(x => x.Replace(propName + ".", "")).ToList();
                            var data = await GetChildListObjectAsync(prop, item, false, childFieldsName, tenantCode);

                            resutlDictionary[prop.Name] = data;
                        }
                    }

                }
            }

            return item;
        }

        #endregion private methods
    }

}

