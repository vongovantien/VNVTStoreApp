using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.Infrastructure.EventBus.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Service
{
    public interface IBaseService<TDataContext, TEntity>
    {
        Task<TEntity> GetByCode(object code, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<object> GetByCodeFields(object code, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        TEntity GetByCodeIncludeChildren(object code, object companyCode = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);

        Task<TEntity> GetByCodeIncludeChildrenAsync(object code, object companyCode = null, List<string> fields = null, string tenantCode = null);
        Task<object> GetByCodeIncludeChildrenFieldAsync(object code, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY,
          List<string> fields = null, string tenantCode = null);

        Task<IEnumerable<TEntity>> GetAllAsync(object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<object> GetAllFieldsAsync(List<string> fields, object companyCode = null, SortDTO sortDTO = null,
          DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);
        IEnumerable<TEntity> GetAll(object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);

        Task<IEnumerable<TEntity>> GetAllIncludeChildrenAsync(object companyCode = null,
            SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<object> GetAllIncludeChildrenFieldsAsync(List<string> fields, object companyCode = null, SortDTO sortDTO = null,
            DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);
        Task<ResultDTO<TEntity>> GetAllPagingAsync(int pageIndex, int pageSize, object companyCode = null,
            SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);

        Task<ResultDTO<object>> GetAllPagingFieldsAsync(int pageIndex, int pageSize, List<string> fields, object companyCode = null,
          SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);

        Task<ResultDTO<TEntity>> GetAllPagingIncludeChildren(int pageIndex, int pageSize, object companyCode = null,
            SortDTO sortDTO = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);

        Task<TEntity> Insert(TEntity objectToInsert);
        Task<TEntity> Insert(TEntity objectToInsert, IntegrationEvent @event, string createdBy = null, EventState eventState = EventState.NotPublished);
        Task<TEntity> Insert(TEntity objectToInsert, Func<Task> function);
        Task<TEntity> Update(TEntity objectToUpdate, bool includeChildren = true);
        Task<TEntity> Update(TEntity objectToUpdate, IntegrationEvent @event, bool includeChildren = true, EventState eventState = EventState.NotPublished);
        Task<TEntity> Update(TEntity objectToUpdate, Func<Task> action, bool includeChildren = true);
        Task<bool> Delete(object code, IntegrationEvent @event, object companyCode = null, EventState eventState = EventState.NotPublished);
        Task<bool> Delete(object code, object companyCode = null);
        Task<bool> Delete(object code, Func<Task> action, object companyCode = null);
        Task<bool> Remove(object code, object companyCode = null);
        Task<bool> Remove(object code, IntegrationEvent @event, object companyCode = null, EventState eventState = EventState.NotPublished);
        Task<bool> Remove(object code, Func<Task> action, object companyCode = null);
        Task<IEnumerable<TEntity>> Search(List<SearchDTO> searchList, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<object> SearchFieldsAysnc(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null, int limit = 0);

        Task<IEnumerable<T>> SearchFieldsAysnc<T>(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null, int limit = 0);

        Task<IEnumerable<TEntity>> SearchIncludeChildren(List<SearchDTO> searchList, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<object> SearchIncludeChildrenFieldAsync(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
         object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY
         , string tenantCode = null);

        Task<object> SearchSingleAsync(List<SearchDTO> searchList, List<string> fields, SortDTO sortDTO = null,
        object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);
        Task<ResultDTO<TEntity>> SearchPaging(List<SearchDTO> searchList, int pageSize, int pageIndex, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);
        Task<ResultDTO<object>> SearchPagingFieldsAsync(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);
        Task<ResultDTO<TEntity>> SearchIncludeChildrenPaging(List<SearchDTO> searchList, int pageSize, int pageIndex, SortDTO sortDTO = null,
            object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, List<string> fields = null, string tenantCode = null);

        Task<ResultDTO<object>> SearchIncludeChildrenPagingFields(List<SearchDTO> searchList, int pageSize,
          int pageIndex, List<string> fields, SortDTO sortDTO = null, object companyCode = null,
          DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<long> CountAsync(List<SearchDTO> searchList,
           DataLevel dataLevel = DataLevel.COMPANY, object companyCode = null, string tenantCode = null);

        /// <summary>
        /// Search Paging Fields Async V2
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
        Task<ResultDTO<object>> SearchPagingFieldsAsyncV2(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);

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
         Task<ResultDTO<object>> SearchPagingFieldsAsyncV3(List<SearchDTO> searchList, int pageSize, int pageIndex, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, int initPages = 5, int extendPages = 2, int rowOffset = 0,
          string tenantCode = null);

        /// <summary>
        /// Count Limit
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="pageSize"></param>
        /// <param name="offset"></param>
        /// <param name="fields"></param>
        /// <param name="sortDTO"></param>
        /// <param name="companyCode"></param>
        /// <param name="dataLevel"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<long> CountLimit(List<SearchDTO> searchList, int pageSize, int offset, List<string> fields,
          SortDTO sortDTO = null, object companyCode = null, DataLevel dataLevel = DataLevel.COMPANY, string tenantCode = null);
    }
}