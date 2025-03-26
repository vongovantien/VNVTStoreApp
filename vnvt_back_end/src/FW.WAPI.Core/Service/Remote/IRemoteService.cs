using FW.WAPI.Core.DAL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Remote
{
    public interface IRemoteService
    {
        Task<ResponseDTO> Get(string servicePath, string tenantCode = null);
        Task<ResponseDTO> Post(string servicePath, object bodyParam, bool isEncrypted = false, string tenantCode = null);
        Task<dynamic> Post(string servicePath, object bodyParam, List<string> fields, bool isEncrypted = false, string tenantCode = null);
        Task<TEntity> Post<TEntity>(string servicePath, object bodyParam, bool isEncrypted = false, string tenantCode = null,
            List<string> fields = null) where TEntity : class;
        Task<TEntity> Post<TEntity>(string servicePath, object bodyParam, int pageSize, int pageIndex, bool isEncrypted = false,
           string tenantCode = null, List<string> fields = null) where TEntity : class;

        Task<ResponseDTO> InsertAsync(string servicePath, object bodyParam, bool isEncrypted = false, string tenantCode = null);
        Task<TEntity> GetByCode<TEntity>(string servicePath, string code, string companyCode = null, List<string> fields = null, string tenantCode = null) where TEntity : class;
        Task<List<TEntity>> GetAll<TEntity>(string servicePath, string companyCode = null, List<string> fields = null, SortDTO sort = null, string tenantCode = null) where TEntity : class;
        Task<List<TEntity>> Search<TEntity>(string servicePath, List<SearchDTO> searches, string companyCode = null, List<string> fields = null,
            SortDTO sort = null, string tenantCode = null) where TEntity : class;

        /// <summary>
        /// Search Single Async
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="searches"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        /// <exception cref="RemoteServiceException"></exception>
        Task<TEntity> SearchSingle<TEntity>(string servicePath, List<SearchDTO> searches,
            string companyCode = null, List<string> fields = null, SortDTO sort = null, string tenantCode = null) where TEntity : class;

        Task<ResultDTO<TEntity>> SearchPaging<TEntity>(string servicePath, List<SearchDTO> searches, string companyCode = null, List<string> fields = null,
            SortDTO sort = null, int? pageIndex = null, int? pageSize = null, string tenantCode = null) where TEntity : class;
        void AddTenantHeader(string tenantCode);
        Task<ResponseDTO> UpdateAsync(string servicePath, object objectToUpdate, bool isEncrypted = false, string tenantCode = null);
        Task<ResponseDTO> DeleteAsync(string servicePath, string code, string companyCode = null, bool isEncrypted = false, string tenantCode = null);
        Task<ResponseDTO> RemoveAsync(string servicePath, string code, string companyCode = null, bool isEncrypted = false, string tenantCode = null);

        /// <summary>
        /// Count Async
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="searches"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        /// <exception cref="RemoteServiceException"></exception>
        Task<long> CountAsync(string servicePath, List<SearchDTO> searches,
            string companyCode = null, string tenantCode = null);

    }
}
