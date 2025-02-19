using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.MediaType;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Remote
{
    public class RemoteService : IRemoteService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _configuration;
        private const string ENCRYPTED_KEY = "encryptedKey";
        public RemoteService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <returns></returns>
        public async Task<ResponseDTO> Get(string servicePath, string tenantCode = null)
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            HttpResponseMessage remoteResponse = await _httpClient.GetAsync(servicePath);

            if (remoteResponse.IsSuccessStatusCode)
            {
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(await remoteResponse.Content.ReadAsStringAsync());
            }
            else
            {
                if (remoteResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AddAuthenResponseHeader(remoteResponse);
                }

                throw new RemoteServiceException(GetType().Name + ": " + remoteResponse.ReasonPhrase, (int)remoteResponse.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="bodyParam"></param>
        /// <returns></returns>
        /// <exception cref="">Throw exception when server error</exception>
        public async Task<ResponseDTO> Post(string servicePath, object bodyParam, bool isEncrypted = false, string tenantCode = null)
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            StringContent postContent;

            if (isEncrypted)
            {
                string key = _configuration.GetValue<string>(ENCRYPTED_KEY);
                var json = JsonUtilities.ConvertObjectToJson(bodyParam);
                var encrypted = SimpleStringCipher.Instance.Encrypt(json, key, Encoding.ASCII.GetBytes(key));
                encrypted = "\"" + encrypted + "\"";
                postContent = new StringContent(encrypted, Encoding.UTF8, MediaTypeName.JSON);
            }
            else
            {
                postContent = new StringContent(JsonUtilities.ConvertObjectToJson(bodyParam), Encoding.UTF8, MediaTypeName.JSON);
            }

            var result = await _httpClient.PostAsync(servicePath, postContent);

            if (result.IsSuccessStatusCode)
            {
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(await result.Content.ReadAsStringAsync());
            }
            else
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AddAuthenResponseHeader(result);
                }

                throw new RemoteServiceException(GetType().Name + ": " + result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="bodyParam"></param>
        /// <param name="fields"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<dynamic> Post(string servicePath, object bodyParam, List<string> fields, bool isEncrypted = false, string tenantCode = null)
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = bodyParam;
            request.Fields = fields;

            var result = await Post(servicePath, request, isEncrypted);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    if (result.Data.GetType().FullName == "System.String")
                    {
                        return result.Data.ToString();
                    }
                    else
                    {
                        return JsonUtilities.ConvertJsonToObject<object>(result.Data.ToString());
                    }
                }

                return null;
            }
            else if (result.Code == 800)
            {
                throw new FriendlyException(result.Message);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="bodyParam"></param>
        /// <param name="isEncrypted"></param>
        /// <returns></returns>
        public async Task<TEntity> Post<TEntity>(string servicePath, object bodyParam, bool isEncrypted = false,
            string tenantCode = null, List<string> fields = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = bodyParam;
            request.Fields = fields;

            var result = await Post(servicePath, request, isEncrypted);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    if (typeof(TEntity).FullName == "System.String")
                    {
                        return result.Data.ToString();
                    }
                    else
                    {
                        return JsonUtilities.ConvertJsonToObject<TEntity>(result.Data.ToString());
                    }
                }

                return null;

            }
            else if (result.Code == 800)
            {
                throw new FriendlyException(result.Message);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }
        }

        public async Task<TEntity> Post<TEntity>(string servicePath, object bodyParam, int pageSize, int pageIndex, bool isEncrypted = false,
           string tenantCode = null, List<string> fields = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = bodyParam;
            request.Fields = fields;
            request.PageIndex = pageIndex;
            request.PageSize = pageSize;

            var result = await Post(servicePath, request, isEncrypted);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    if (typeof(TEntity).FullName == "System.String")
                    {
                        return result.Data.ToString();
                    }
                    else
                    {
                        return JsonUtilities.ConvertJsonToObject<TEntity>(result.Data.ToString());
                    }
                }

                return null;

            }
            else if (result.Code == 800)
            {
                throw new FriendlyException(result.Message);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByCode<TEntity>(string servicePath, string code, string companyCode = null,
            List<string> fields = null, string tenantCode = null) where TEntity : class
        {

            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();

            if (!string.IsNullOrEmpty(companyCode))
            {
                request.PostObject = new { Code = code, CompanyCode = companyCode };
            }
            else
            {
                request.PostObject = code;
            }

            request.Fields = fields;
            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    TEntity entity = JsonUtilities.ConvertJsonToObject<TEntity>(result.Data.ToString());
                    return entity;
                }
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetAll<TEntity>(string servicePath, string companyCode = null,
            List<string> fields = null, SortDTO sort = null, string tenantCode = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = companyCode;

            request.Fields = fields;
            request.SortDTO = sort;
            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    List<TEntity> entity = JsonUtilities.ConvertJsonToObject<List<TEntity>>(result.Data.ToString());
                    return entity;
                }
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="searches"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> Search<TEntity>(string servicePath, List<SearchDTO> searches,
            string companyCode = null, List<string> fields = null, SortDTO sort = null, string tenantCode = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = companyCode;
            request.Searching = searches;
            request.Fields = fields;
            request.SortDTO = sort;
            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    List<TEntity> entity = JsonUtilities.ConvertJsonToObject<List<TEntity>>(result.Data.ToString());
                    return entity;
                }
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }

            return null;
        }

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
        public async Task<TEntity> SearchSingle<TEntity>(string servicePath, List<SearchDTO> searches,
            string companyCode = null, List<string> fields = null, SortDTO sort = null, string tenantCode = null) where TEntity : class
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = companyCode;
            request.Searching = searches;
            request.Fields = fields;
            request.SortDTO = sort;
            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    TEntity entity = JsonUtilities.ConvertJsonToObject<TEntity>(result.Data.ToString());
                    return entity;
                }
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="bodyParam"></param>
        /// <param name="isEncrypted"></param>
        /// <returns></returns>
        public async Task<ResponseDTO> InsertAsync(string servicePath, object bodyParam, bool isEncrypted = false, string tenantCode = null)
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = bodyParam;

            StringContent postContent;

            if (isEncrypted)
            {
                string key = _configuration.GetValue<string>(ENCRYPTED_KEY);
                var json = JsonUtilities.ConvertObjectToJson(request);
                var encrypted = SimpleStringCipher.Instance.Encrypt(json, key, Encoding.ASCII.GetBytes(key));
                encrypted = "\"" + encrypted + "\"";
                postContent = new StringContent(encrypted, Encoding.UTF8, MediaTypeName.JSON);
            }
            else
            {
                postContent = new StringContent(JsonUtilities.ConvertObjectToJson(request), Encoding.UTF8, MediaTypeName.JSON);
            }

            var result = await _httpClient.PostAsync(servicePath, postContent);

            if (result.IsSuccessStatusCode)
            {
                var resultData = await result.Content.ReadAsStringAsync();
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(resultData);
            }
            else
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AddAuthenResponseHeader(result);
                }

                throw new RemoteServiceException(GetType().Name + ": " + result.ReasonPhrase, (int)result.StatusCode);
            }
        }



        /// <summary>
        /// Add tenant code to httpclient
        /// </summary>
        /// <param name="tenantCode"></param>
        public void AddTenantHeader(string tenantCode)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(MultiTenancyConsts.TenantIdResolveKey))
            {
                _httpClient.DefaultRequestHeaders.Remove(MultiTenancyConsts.TenantIdResolveKey);
            }

            _httpClient.DefaultRequestHeaders.Add(MultiTenancyConsts.TenantIdResolveKey, tenantCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="objectToUpdate"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ResponseDTO> UpdateAsync(string servicePath, object objectToUpdate,
            bool isEncrypted = false, string tenantCode = null)
        {
            var request = new RequestDTO();
            request.PostObject = objectToUpdate;


            StringContent postContent;

            if (isEncrypted)
            {
                string key = _configuration.GetValue<string>(ENCRYPTED_KEY);
                var json = JsonUtilities.ConvertObjectToJson(request);
                var encrypted = SimpleStringCipher.Instance.Encrypt(json, key, Encoding.ASCII.GetBytes(key));

                postContent = new StringContent(encrypted, Encoding.UTF8, MediaTypeName.JSON);
            }
            else
            {
                postContent = new StringContent(JsonUtilities.ConvertObjectToJson(request), Encoding.UTF8, MediaTypeName.JSON);
            }

            var result = await _httpClient.PostAsync(servicePath, postContent);

            if (result.IsSuccessStatusCode)
            {
                var resultData = await result.Content.ReadAsStringAsync();
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(resultData);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ResponseDTO> DeleteAsync(string servicePath, string code, string companyCode = null,
            bool isEncrypted = false, string tenantCode = null)
        {
            var request = new RequestDTO();

            if (string.IsNullOrEmpty(companyCode))
            {
                request.PostObject = new { Code = code };
            }
            else
            {
                request.PostObject = new { Code = code, CompanyCode = companyCode };
            }

            StringContent postContent;

            if (isEncrypted)
            {
                string key = _configuration.GetValue<string>(ENCRYPTED_KEY);
                var json = JsonUtilities.ConvertObjectToJson(request);
                var encrypted = SimpleStringCipher.Instance.Encrypt(json, key, Encoding.ASCII.GetBytes(key));

                postContent = new StringContent(encrypted, Encoding.UTF8, MediaTypeName.JSON);
            }
            else
            {
                postContent = new StringContent(JsonUtilities.ConvertObjectToJson(request), Encoding.UTF8, MediaTypeName.JSON);
            }

            var result = await _httpClient.PostAsync(servicePath, postContent);

            if (result.IsSuccessStatusCode)
            {
                var resultData = await result.Content.ReadAsStringAsync();
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(resultData);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="code"></param>
        /// <param name="companyCode"></param>
        /// <param name="isEncrypted"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ResponseDTO> RemoveAsync(string servicePath, string code, string companyCode = null,
            bool isEncrypted = false, string tenantCode = null)
        {
            var request = new RequestDTO();

            if (string.IsNullOrEmpty(companyCode))
            {
                request.PostObject = new { Code = code };
            }
            else
            {
                request.PostObject = new { Code = code, CompanyCode = companyCode };
            }

            StringContent postContent;

            if (isEncrypted)
            {
                string key = _configuration.GetValue<string>(ENCRYPTED_KEY);
                var json = JsonUtilities.ConvertObjectToJson(request);
                var encrypted = SimpleStringCipher.Instance.Encrypt(json, key, Encoding.ASCII.GetBytes(key));

                postContent = new StringContent(encrypted, Encoding.UTF8, MediaTypeName.JSON);
            }
            else
            {
                postContent = new StringContent(JsonUtilities.ConvertObjectToJson(request), Encoding.UTF8, MediaTypeName.JSON);
            }

            var result = await _httpClient.PostAsync(servicePath, postContent);

            if (result.IsSuccessStatusCode)
            {
                var resultData = await result.Content.ReadAsStringAsync();
                return JsonUtilities.ConvertJsonToObject<ResponseDTO>(resultData);
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="servicePath"></param>
        /// <param name="searches"></param>
        /// <param name="companyCode"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<ResultDTO<TEntity>> SearchPaging<TEntity>(string servicePath, List<SearchDTO> searches, string companyCode = null,
            List<string> fields = null, SortDTO sort = null, int? pageIndex = null, int? pageSize = null, string tenantCode = null) where TEntity : class
        {
            var resultDTO = new ResultDTO<TEntity>();
            resultDTO.Total = 0;
            resultDTO.Data = null;

            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();

            request.PostObject = companyCode;
            request.Searching = searches;
            request.Fields = fields;
            request.SortDTO = sort;
            request.PageIndex = pageIndex;
            request.PageSize = pageSize;

            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                if (result.Data != null)
                {
                    List<TEntity> entity = JsonUtilities.ConvertJsonToObject<List<TEntity>>(result.Data.ToString());
                    resultDTO.Data = entity;
                    resultDTO.Total = result.Total;

                    return resultDTO;
                }
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }

            return resultDTO;
        }

        /// <summary>
        /// Count Async
        /// </summary>
        /// <param name="servicePath"></param>
        /// <param name="searches"></param>
        /// <param name="companyCode"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        /// <exception cref="RemoteServiceException"></exception>
        public async Task<long> CountAsync(string servicePath, List<SearchDTO> searches,
            string companyCode = null, string tenantCode = null)
        {
            if (!string.IsNullOrEmpty(tenantCode))
            {
                AddTenantHeader(tenantCode);
            }

            var request = new RequestDTO();
            request.PostObject = companyCode;
            request.Searching = searches;
            var result = await Post(servicePath, request);

            if (result.Code == 900)
            {
                return result.Data ?? 0;
            }
            else
            {
                throw new RemoteServiceException(GetType().Name + ": " + result.Message, result.Code);
            }
        }


        /// <summary>
        /// Add Authen Response Header
        /// </summary>
        /// <param name="remoteResponse"></param>
        private void AddAuthenResponseHeader(HttpResponseMessage remoteResponse)
        {
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();

            if (httpContextAccessor.HttpContext.Response.Headers.ContainsKey(HeaderNames.WWWAuthenticate))
            {
                httpContextAccessor.HttpContext.Response.Headers.Remove(HeaderNames.WWWAuthenticate);
            }

            var headerAuthen = remoteResponse.Headers.WwwAuthenticate.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(headerAuthen))
            {
                httpContextAccessor.HttpContext.Response.Headers.Add(HeaderNames.WWWAuthenticate, headerAuthen);
            }
        }
    }
}
