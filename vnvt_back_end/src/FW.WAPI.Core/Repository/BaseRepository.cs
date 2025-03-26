using Dapper;
using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FW.WAPI.Core.DAL.Model.Tenant;
using FW.WAPI.Core.MultiTenancy;
using FW.WAPI.Core.Domain.Context;
using System.Dynamic;
using FW.WAPI.Core.Infrastructure.Logger;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FW.WAPI.Core.Repository
{
    public class BaseRepository<TDataContext, TEntity> : IRepository<TDataContext, TEntity>
        where TDataContext : DbContext
        where TEntity : class
    {

        #region Private member variables...
        private readonly IConfiguration _configuration;
        private DbConnection _dbConnection;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICoreLogger _coreLogger;
        public string ConnectionString { get; set; }

        private readonly IStartupCoreOptions _startupCoreOptions;
        private readonly ITenantStore _tenantStore;

        internal DbSet<TEntity> DbSet;

        #endregion Private member variables...

        public DbSet<TEntity> Root
        {
            get
            {
                return DbSet;
            }
        }

        public TDataContext DataContext { get; set; }


        #region Public Constructor...

        /// <summary>
        /// Public Constructor,initializes privately declared local variables.
        /// </summary>
        /// <param name="context"></param>
        public BaseRepository(TDataContext context, IConfiguration configuration,
            IStartupCoreOptions startupCoreOptions, IServiceProvider serviceProvider)
        {
            DataContext = context;

            DbSet = context.Set<TEntity>();
            _configuration = configuration;
            _dbConnection = DataContext.Database.GetDbConnection();
            _startupCoreOptions = startupCoreOptions;
            ConnectionString = _dbConnection.ConnectionString;

            _serviceProvider = serviceProvider;

            if (startupCoreOptions.IsMultyTenancy)
            {
                _tenantStore = (ITenantStore)serviceProvider.GetRequiredService(typeof(ITenantStore));
            }

            _coreLogger = _serviceProvider.GetRequiredService<ICoreLogger>();
        }


        #endregion Public Constructor...

        #region Public member methods...

        /// <summary>
        /// Save method.
        /// </summary>
        public void SaveChanges()
        {
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Save Async method.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            var count = await DataContext.SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// generic Insert method for the entities
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual async Task InsertRangeAsync(List<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// Generic update method for the entities
        /// </summary>
        /// <param name="entityToUpdate"></param>
        public virtual void Update(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            DataContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="entityToUpdate"></param>
        public void Update(TEntity oldEntity, TEntity entityToUpdate)
        {
            DataContext.Entry(oldEntity).CurrentValues.SetValues(entityToUpdate);
        }

        /// <summary>
        /// Generic Delete method for the entities
        /// </summary>
        /// <param name="id"></param>
        public virtual void Delete(object id)
        {
            TEntity entityToDelete = DbSet.Find(id);
            Delete(entityToDelete);
        }

        /// <summary>
        /// generic delete method , deletes data for the entities on the basis of condition.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public void Delete(Func<TEntity, bool> where)
        {
            IQueryable<TEntity> objects = DbSet.Where(where).AsQueryable();
            foreach (TEntity obj in objects)
                DbSet.Remove(obj);
        }

        /// <summary>
        /// Generic Delete method for the entities
        /// </summary>
        /// <param name="entityToDelete"></param>
        public virtual void Delete(TEntity entityToDelete)
        {
            if (DataContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                DbSet.Attach(entityToDelete);
            }

            DbSet.Remove(entityToDelete);
        }

        /// <summary>
        /// Generic get method on the basis of id for Entities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity GetByID(object id)
        {
            return DbSet.Find(id);
        }

        /// <summary>
        /// generic method to get many record on the basis of a condition.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false, string tenantCode = null)
        {
            if (tenantCode is null)
            {
                IQueryable<TEntity> query = asNoTracking ? DbSet.AsNoTracking() : DbSet;

                if (where != null)
                {
                    query = query.Where(where);
                }

                if (orderBy != null)
                {
                    return orderBy(query.AsQueryable());
                }

                return query.ToList();
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();
                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                        typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        IQueryable<TEntity> query = asNoTracking ? dbContext.Set<TEntity>().AsNoTracking() : dbContext.Set<TEntity>();

                        if (where != null)
                        {
                            query = query.Where(where);
                        }

                        if (orderBy != null)
                        {
                            return orderBy(query.AsQueryable());
                        }

                        return query.ToList();
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false, string tenantCode = null)
        {
            if (tenantCode is null)
            {
                IQueryable<TEntity> query = asNoTracking ? DbSet.AsNoTracking() : DbSet;

                if (where != null)
                {
                    query = query.Where(where);
                }

                if (orderBy != null)
                {
                    return orderBy(query.AsQueryable());
                }

                return await query.ToListAsync();
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();

                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                        typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        IQueryable<TEntity> query = asNoTracking ? dbContext.Set<TEntity>().AsNoTracking() : dbContext.Set<TEntity>();

                        if (where != null)
                        {
                            query = query.Where(where);
                        }

                        if (orderBy != null)
                        {
                            return orderBy(query.AsQueryable());
                        }

                        return await query.ToListAsync();
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }
        }

        /// <summary>
        /// generic method to get many record on the basis of a condition but query able.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetManyQueryable(Func<TEntity, bool> where)
        {
            return DbSet.Where(where).AsQueryable();
        }

        /// <summary>
        /// generic get method , fetches data for the entities on the basis of condition.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> where, bool asNoTracking = false, string tenantCode = null)
        {
            if (tenantCode is null)
            {
                if (asNoTracking)
                {
                    return DbSet.AsNoTracking().FirstOrDefault(where);
                }
                else
                {
                    return DbSet.FirstOrDefault(where);
                }
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();
                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                    typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        var result = asNoTracking ? dbContext.Set<TEntity>().AsNoTracking().FirstOrDefault(where)
                            : dbContext.Set<TEntity>().FirstOrDefault(where);

                        return result;
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where, bool asNoTracking = false, string tenantCode = null)
        {
            //return await DbSet.FirstOrDefaultAsync(where);

            if (tenantCode is null)
            {
                if (asNoTracking)
                {
                    return await DbSet.AsNoTracking().FirstOrDefaultAsync(where);
                }
                else
                {
                    return await DbSet.FirstOrDefaultAsync(where);
                }
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();

                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                    typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        var result = asNoTracking ? dbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(where)
                            : dbContext.Set<TEntity>().FirstOrDefaultAsync(where);

                        return await result;
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }
        }

        /// <summary>
        /// generic method to fetch all the records from db
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetAll(bool asNoTracking = false, string tenantCode = null)
        {
            if (tenantCode is null)
            {
                if (asNoTracking)
                {
                    return DbSet.AsNoTracking().ToList();
                }
                else
                {
                    return DbSet.ToList();
                }
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();

                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                    typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        var result = asNoTracking ? dbContext.Set<TEntity>().AsNoTracking().ToList()
                            : dbContext.Set<TEntity>().ToList();

                        return result;
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asNoTracking"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = false, string tenantCode = null)
        {
            if (tenantCode is null)
            {
                if (asNoTracking)
                {
                    return await DbSet.AsNoTracking().ToListAsync();
                }
                else
                {
                    return await DbSet.ToListAsync();
                }
            }
            else
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);

                if (tenant != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);

                    var dboptions = new DbContextOptionsBuilder<TDataContext>();
                    if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.MSSQL)
                    {
                        dboptions.UseSqlServer(connectionString);
                    }
                    else if (_startupCoreOptions.DatabaseProvider == EnumTypes.DatabaseProvider.POSTGRESQL)
                    {
                        dboptions.UseNpgsql(connectionString);
                    }

                    using (TDataContext dbContext = (TDataContext)ActivatorUtilities.CreateInstance(_serviceProvider,
                    typeof(TDataContext), new object[] { dboptions.Options }))
                    {
                        var result = asNoTracking ? await dbContext.Set<TEntity>().AsNoTracking().ToListAsync()
                            : await dbContext.Set<TEntity>().ToListAsync();

                        return result;
                    }
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }
        }

        /// <summary>
        /// Inclue multiple
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public IQueryable<TEntity> GetWithInclude(Expression<Func<TEntity, bool>> predicate, params string[] include)
        {
            IQueryable<TEntity> query = this.DbSet;
            query = include.Aggregate(query, (current, inc) => current.Include(inc));
            return query.Where(predicate);
        }

        /// <summary>
        /// Generic method to check if entity exists
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public bool Exists(object primaryKey)
        {
            return DbSet.Find(primaryKey) != null;
        }

        /// <summary>
        /// Gets a single record by the specified criteria (usually the unique identifier)
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record that matches the specified criteria</returns>
        public TEntity GetSingle(Func<TEntity, bool> predicate)
        {
            return DbSet.FirstOrDefault<TEntity>(predicate);
        }

        /// <summary>
        /// The first record matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record containing the first record matching the specified criteria</returns>
        public TEntity GetFirst(Func<TEntity, bool> predicate)
        {
            return DbSet.First(predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="TenantNotFoundException"></exception>
        public async Task<IEnumerable<T>> RawQueryMultipleAsync<T>(string query, string tenantCode = null, object param = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = await connection.QueryAsync<T>(query, param);

                return results;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> RawQueryMultipleDynamicAsync(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = await connection.QueryAsync<dynamic>(query);
                return results.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> RawQueryAsync(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var stopWatch = Stopwatch.StartNew();

                try
                {
                    var results = await connection.QueryAsync(query);
                    stopWatch.Stop();
                    return results;
                }
                finally
                {
                    _coreLogger.Log(LogLevel.Information, $"RawQueryAsync: {query}, Duration: {stopWatch.Elapsed.TotalMilliseconds}ms");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> RawQueryFirstAsync(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var stopWatch = Stopwatch.StartNew();

                try
                {
                    var results = await connection.QueryFirstOrDefaultAsync(query);
                    stopWatch.Stop();
                    return results;
                }
                finally
                {
                    _coreLogger.Log(LogLevel.Information, $"RawQueryFirstAsync: {query}, Duration: {stopWatch.Elapsed.TotalMilliseconds}ms");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public IEnumerable<T> RawQueryMultiple<T>(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)_tenantStore.GetTenant(tenantCode);
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = connection.Query<T>(query);
                _coreLogger.Log(query);
                return results.ToList();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="query"></param>
        /// <param name="searchDTO"></param>
        /// <returns></returns>
        public async Task<TEntity> RawQuerySingleAsync(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var result = await connection.QueryFirstOrDefaultAsync<TEntity>(query);
                _coreLogger.Log(query);
                return result;
            }
        }

        /// <summary>
        /// Raw Query Single With Param Async
        /// </summary>
        /// <param name="query"></param>
        /// <param name="param"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TEntity> RawQuerySingleWithParamAsync(string query, object param, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var result = await connection.QueryFirstOrDefaultAsync<TEntity>(query, param);
                _coreLogger.Log(query);
                return result;
            }
        }

        /// <summary>
        /// Raw Query Single With Param Async
        /// </summary>
        /// <param name="query"></param>
        /// <param name="param"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<T> RawQuerySingleWithParamAsync<T>(string query, object param, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(query, param);
                _coreLogger.Log(query);
                return result;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public TEntity RawQuerySingle(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(_tenantStore.GetTenant(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var result = connection.QueryFirstOrDefault<TEntity>(query);
                _coreLogger.Log(query);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> RawQueryDynamicSingle(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {

                var results = await connection.QueryFirstOrDefaultAsync<dynamic>(query);
                _coreLogger.Log(query);
                return results;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> RawQueryDynamicSingle(string query, object param, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = await connection.QueryFirstOrDefaultAsync<dynamic>(query, param);
                _coreLogger.Log(query);
                return results;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ExpandoObject> RawQueryObjectSingle(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var result = await connection.QueryAsync<ExpandoObject>(query);
                _coreLogger.Log(query);
                return result.ToList().FirstOrDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public dynamic RawQueryMultipleDynamic(string query, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(_tenantStore.GetTenant(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = connection.Query<dynamic>(query);
                _coreLogger.Log(query);
                return results.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task InsertAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <returns></returns>
        public Task UpdateAsync(TEntity entityToUpdate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="entityToUpdate"></param>
        /// <returns></returns>
        public Task UpdateAsync(TEntity oldEntity, TEntity entityToUpdate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Task DeleteAsync(Func<TEntity, bool> where)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task DeleteAsync(object id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityToDelete"></param>
        /// <returns></returns>
        public Task DeleteAsync(TEntity entityToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteStoreProcedure<T>(string storedProcedureName,
            object param = null, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = await connection.QueryAsync<T>(storedProcedureName,
                    param, null, null, System.Data.CommandType.StoredProcedure);

                return results.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleStoreProcedure<T>(string storedProcedureName, object param = null, string tenantCode = null)
        {
            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));

                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }

            using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
            {
                var results = await connection.QueryFirstOrDefaultAsync<T>(storedProcedureName,
                    param, null, null, System.Data.CommandType.StoredProcedure);

                return results;
            }
        }

        /// <summary>
        /// Generate unique code for tentity
        /// </summary>
        /// <remarks></remarks>
        /// <param name="companyCode"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async Task<string> GenerateUniqueCode(string companyCode = null, string prefix = null, string tenantCode = null)
        {
            string newCode;

            int maxLen = 8;

            object item;
            var mapping = DataContext.Model.FindEntityType(typeof(TEntity).FullName).Relational();
            var tableName = mapping.TableName;

            var connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                var tenant = (IBaseTenant)(await _tenantStore.GetTenantAsync(tenantCode));
                if (tenant != null)
                {
                    connectionString = SimpleStringCipher.Instance.Decrypt(tenant.ConnectionString);
                }
                else
                {
                    throw new TenantNotFoundException("Tenant code does not exist");
                }
            }

            if (prefix == null)
            {
                if (_startupCoreOptions.SystemItemListType != null)
                {
                    var systemItemListType = _serviceProvider.GetService(_startupCoreOptions.SystemItemListType);

                    if (systemItemListType is IGenerateUniqueCodeAsync uniqueCodeAsync)
                    {
                        var autoSettingValue = await uniqueCodeAsync.GetUniqueCodePrefixAsync(tableName, companyCode);

                        if (autoSettingValue != null)
                        {
                            prefix = autoSettingValue.PrefixOfDefaultValueForCode;

                            if (autoSettingValue.LengthOfDefaultValueForCode.HasValue)
                            {
                                maxLen = autoSettingValue.LengthOfDefaultValueForCode.Value;
                            }
                        }
                        else
                        {
                            prefix = GeneralConst.CODE_PREFIX_DEFAULT;
                        }

                    }
                }
                else
                {
                    if (DataContext is IGenerateUniqueCode uniqueCode)
                    {
                        prefix = uniqueCode.GetUniqueCodePrefix(tableName, ref maxLen, companyCode);
                    }
                    else
                    {
                        //set default prefix if not implement inteface
                        prefix = GeneralConst.CODE_PREFIX_DEFAULT;
                    }
                }
            }

            if (companyCode == null)
            {
                //TuanNT
                var query = string.Format("SELECT * FROM {1} WHERE \"Code\" LIKE '%{0}%' ORDER BY \"Code\" DESC", prefix + GeneralConst.CODE_PREFIX_DELIMITER, tableName);

                using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
                {
                    var queryResult = await connection.QueryAsync<TEntity>(query);
                    item = queryResult.FirstOrDefault();
                }
            }
            else
            {
                //TuanNT
                var query = string.Format("SELECT * FROM {2} WHERE \"Code\" LIKE '%{1}%' and \"CompanyCode\" = '{0}' ORDER BY \"Code\" DESC",
                   companyCode, prefix + GeneralConst.CODE_PREFIX_DELIMITER, tableName);

                using (var connection = DbConnectionFactory.CreateDbConnection(_startupCoreOptions.DatabaseProvider, connectionString))
                {
                    var queryResult = await connection.QueryAsync<TEntity>(query);
                    item = queryResult.FirstOrDefault();
                }
            }

            if (item == null)
            {
                newCode = "0";
            }
            else
            {
                string code = item.GetType().GetProperty("Code").GetValue(item, null).ToString();
                try
                {
                    int maxId;

                    int idx = code.IndexOf(GeneralConst.CODE_PREFIX_DELIMITER);

                    if (idx > -1)
                    {
                        maxId = int.Parse(code.Remove(0, idx + 1));
                    }
                    else
                    {
                        maxId = int.Parse(code);
                    }

                    newCode = (maxId + 1).ToString();
                }
                catch
                {
                    newCode = "1".PadLeft(maxLen, '0');
                }
            }

            if (newCode.Length < maxLen)
            {
                newCode = newCode.PadLeft(maxLen, '0');
            }

            return string.IsNullOrEmpty(prefix) ? newCode : string.Format("{0}.{1}", prefix, newCode);
        }


        #endregion Public member methods...
    }
}