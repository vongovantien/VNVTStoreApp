using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Repository
{
    public interface IRepository<TDataContext, TEntity>
        where TDataContext : DbContext
        where TEntity : class
    {

        TDataContext DataContext { get; }
        DbSet<TEntity> Root { get; }

        void Insert(TEntity entity);

        Task InsertAsync(TEntity entity);

        Task InsertRangeAsync(List<TEntity> entities);

        void Update(TEntity entityToUpdate);

        void Update(TEntity oldEntity, TEntity entityToUpdate);

        //Task UpdateAsync(TEntity entityToUpdate);

        //Task UpdateAsync(TEntity oldEntity, TEntity entityToUpdate);

        void Delete(Func<TEntity, bool> where);

        void Delete(object id);

        void Delete(TEntity entityToDelete);

        //Task DeleteAsync(Func<TEntity, bool> where);

        //Task DeleteAsync(object id);

        //Task DeleteAsync(TEntity entityToDelete);

        void SaveChanges();

        Task<int> SaveChangesAsync();

        bool Exists(object primaryKey);

        TEntity Get(Expression<Func<TEntity, bool>> where, bool asNoTracking = false, string tenantCode = null);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where, bool asNoTracking = false, string tenantCode = null);

        IEnumerable<TEntity> GetAll(bool asNoTracking = false, string tenantCode = null);

        Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = false, string tenantCode = null);

        TEntity GetByID(object id);

        TEntity GetFirst(Func<TEntity, bool> predicate);

        IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false, string tenantCode = null);

        Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false, string tenantCode = null);

        IQueryable<TEntity> GetManyQueryable(Func<TEntity, bool> where);

        TEntity GetSingle(Func<TEntity, bool> predicate);

        IQueryable<TEntity> GetWithInclude(Expression<Func<TEntity, bool>> predicate, params string[] include);

        Task<IEnumerable<T>> RawQueryMultipleAsync<T>(string query, string tenantCode = null, object param = null);

        IEnumerable<T> RawQueryMultiple<T>(string query, string tenantCode = null);

        Task<dynamic> RawQueryMultipleDynamicAsync(string query, string tenantCode = null);
        Task<dynamic> RawQueryAsync(string query, string tenantCode = null);
        Task<dynamic> RawQueryFirstAsync(string query, string tenantCode = null);
        dynamic RawQueryMultipleDynamic(string query, string tenantCode = null);

        Task<TEntity> RawQuerySingleAsync(string query, string tenantCode = null);

        Task<TEntity> RawQuerySingleWithParamAsync(string query, object param, string tenantCode = null);

        Task<T> RawQuerySingleWithParamAsync<T>(string query, object param, string tenantCode = null);

        TEntity RawQuerySingle(string query, string tenantCode = null);

        Task<dynamic> RawQueryDynamicSingle(string query, string tenantCode = null);
        Task<dynamic> RawQueryDynamicSingle(string query, object param, string tenantCode = null);

        Task<List<T>> ExecuteStoreProcedure<T>(string storedProcedureName, object param = null, string tenantCode = null);
        Task<T> ExecuteSingleStoreProcedure<T>(string storedProcedureName, object param = null, string tenantCode = null);
        string ConnectionString { get; set; }
        Task<string> GenerateUniqueCode(string companyCode = null, string prefix = null, string tenantCode = null);
    }
}