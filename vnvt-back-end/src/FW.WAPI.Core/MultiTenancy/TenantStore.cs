using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.DAL.Model.Tenant;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using FW.WAPI.Core.MultiTenancy.Resolver;
using FW.WAPI.Core.Service.MultyTenancy;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FW.WAPI.Core.MultiTenancy
{
    public class TenantStore<Tenant> : ITenantStore
      where Tenant : class, IBaseTenant
    {
        private readonly ITenantCoreService<Tenant> _tenantCoreService;

        private readonly IStartupCoreOptions _startupCoreOptions;
        private readonly ITenantResolver _tenantResolver;
        private readonly IMemoryCache _cache;
        private const int CACHED_TIME_OUT = 3600; //3600s = 1hour

        public TenantStore(ITenantCoreService<Tenant> tenantCoreService,
            IStartupCoreOptions startupCoreOptions, ITenantResolver tenantResolver,
            IMemoryCache memoryCache)
        {
            _tenantCoreService = tenantCoreService;
            _startupCoreOptions = startupCoreOptions;
            _tenantResolver = tenantResolver;
            _cache = memoryCache;
        }

        /// <summary>
        ///
        /// </summary>
        public async Task RegisterTenant()
        {
            try
            {
                if (_startupCoreOptions.IsMultyTenancy)
                {
                    var tenantCode = _tenantResolver.ResolveTenantId();

                    if (!string.IsNullOrEmpty(tenantCode))
                    {
                        //Get tenant from cache
                        object cachedTenant = null;
                        _cache.TryGetValue($"__{tenantCode}", out cachedTenant);

                        Tenant tenantInfo = null;
                        if (cachedTenant == null)
                        {
                            tenantInfo = await _tenantCoreService.GetTenantByCode(tenantCode);
                            //set to cache
                            _cache.Set($"__{tenantCode}", tenantInfo, new MemoryCacheEntryOptions()
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHED_TIME_OUT)
                            });
                        }
                        else
                        {
                            tenantInfo = (Tenant)cachedTenant;
                        }

                        if (tenantInfo != null)
                        {
                            var connectionString = SimpleStringCipher.Instance.Decrypt(tenantInfo.ConnectionString);
                            _startupCoreOptions.ConnectionString = connectionString;
                        }
                        else
                        {
                            throw new TenantNotFoundException("Not found tenant");
                        }

                    }
                    else
                    {
                        _startupCoreOptions.ConnectionString = null;
                    }
                }
            }
            catch
            {
                throw new TenantNotFoundException("Not found tenant");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<object> GetTenantAsync(string tenantCode)
        {
            var tenant = await _tenantCoreService.GetTenantByCode(tenantCode);

            return tenant;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public object GetTenant(string tenantCode)
        {
            return _tenantCoreService.GetTenant(tenantCode);
        }


        /// <summary>
        /// Regist Tenant
        /// </summary>
        public async Task RegisterTenant(string tenantCode)
        {
            try
            {
                if (!_startupCoreOptions.IsMultyTenancy) return;

                if (string.IsNullOrEmpty(tenantCode))
                {
                    _startupCoreOptions.ConnectionString = null;
                    return;
                }

                //Get tenant from cache
                object cachedTenant = null;
                _cache.TryGetValue($"__{tenantCode}", out cachedTenant);

                Tenant tenantInfo = null;
                if (cachedTenant == null)
                {
                    tenantInfo = await _tenantCoreService.GetTenantByCode(tenantCode);
                    //set to cache
                    _cache.Set($"__{tenantCode}", tenantInfo, new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHED_TIME_OUT)
                    });
                }
                else
                {
                    tenantInfo = (Tenant)cachedTenant;
                }

                if (tenantInfo != null)
                {
                    var connectionString = SimpleStringCipher.Instance.Decrypt(tenantInfo.ConnectionString);
                    _startupCoreOptions.ConnectionString = connectionString;
                    _startupCoreOptions.TenantCode = tenantInfo.Code;
                }
                else
                {
                    throw new TenantNotFoundException("Not found tenant");
                }
            }
            catch
            {
                throw new TenantNotFoundException("Not found tenant");
            }

        }

        /// <summary>
        /// Cache All Tenant Active
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> CacheAllTenantActive()
        {

            List<Tenant> listTenant = await _tenantCoreService.GetAllActive();
            foreach (Tenant tenantInfo in listTenant)
            {
                string tenantCode = tenantInfo.Code;

                //Get tenant from cache
                object cachedTenant = null;
                _cache.TryGetValue($"__{tenantCode}", out cachedTenant);

                if (cachedTenant == null)
                {
                    //set to cache
                    _cache.Set($"__{tenantCode}", tenantInfo, new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHED_TIME_OUT)
                    });
                }
            }

            var listTenantCode = listTenant.Select(x => x.Code).ToList();
            return listTenantCode;
        }
    }
}