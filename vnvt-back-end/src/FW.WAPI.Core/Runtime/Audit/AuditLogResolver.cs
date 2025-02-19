using FW.WAPI.Core.Attribute;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.Filter;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Service.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Runtime.Audit
{
    public class AuditLogResolver<TDataContext, TAuditLog> : IAuditLogResolver
        where TAuditLog : class
        where TDataContext : DbContext
    {
        private IClientInfoProvider httpContextClientInfoProvider;
        private IAuditLogService<TDataContext, TAuditLog> _auditLogService;
        private IConfiguration _configuration;

        public AuditLogResolver(IAuditLogService<TDataContext,
            TAuditLog> auditLogService,
            IClientInfoProvider clientInfoProvider,
            IConfiguration configuration)
        {
            httpContextClientInfoProvider = clientInfoProvider;
            _auditLogService = auditLogService;
            _configuration = configuration;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public AuditLogInfo CreateAuditInfo(Type type, MethodInfo method, object[] arguments)
        {
            var serviceName = _configuration.GetSection("ServiceName").Value;
            serviceName = string.IsNullOrEmpty(serviceName) ? Guid.NewGuid().ToString().ToLower()
                : serviceName;

            var auditInfo = new AuditLogInfo
            {
                BrowserInfo = httpContextClientInfoProvider.BrowserInfo,
                ClientIpAddress = httpContextClientInfoProvider.ClientIpAddress,
                ClientName = httpContextClientInfoProvider.ComputerName,
                MethodName = method.Name,
                Parameters = arguments.ToString(),
                ExecutionTime = DateTime.Now,
                ServiceName = serviceName
            };

            return auditInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public AuditLogInfo CreateAuditInfo(Type type, MethodInfo method, IDictionary<string, object> arguments)
        {
            var serviceName = _configuration.GetSection("ServiceName").Value;
            serviceName = string.IsNullOrEmpty(serviceName) ? Guid.NewGuid().ToString().ToLower()
                : serviceName;

            var auditInfo = new AuditLogInfo
            {
                BrowserInfo = httpContextClientInfoProvider.BrowserInfo,
                ClientIpAddress = httpContextClientInfoProvider.ClientIpAddress,
                ControllerName = type.ToString(),
                ClientName = httpContextClientInfoProvider.ComputerName,
                MethodName = method.Name,
                Parameters = ConvertArgumentsToJson(arguments),
                ExecutionTime = DateTime.Now,
                ServiceName = _configuration.GetSection("ServiceName").Value
            };

            return auditInfo;
        }

        public void Save(AuditLogInfo auditInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAsync(AuditLogInfo auditInfo)
        {
            TAuditLog auditLog = Utilities.ConvertObjectToObject<TAuditLog>(auditInfo);
            var result = await _auditLogService.SaveAudit(auditLog);

            return result;
        }

        public bool ShouldSaveAudit(MethodInfo methodInfo, bool defaultValue = false)
        {
            if (methodInfo == null)
            {
                return false;
            }

            if (!methodInfo.IsPublic)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(AuditedAttribute), true))
            {
                var auditAttribute = methodInfo.GetCustomAttribute<AuditedAttribute>();

                return auditAttribute.IsEnable;
            }

            var classType = methodInfo.DeclaringType;

            if (classType != null)
            {
                if (classType.GetTypeInfo().IsDefined(typeof(AuditedAttribute), true))
                {
                    var auditAttribute = classType.GetCustomAttribute<AuditedAttribute>();
                    return auditAttribute.IsEnable;
                }
                else
                {
                    return false;
                }
            }

            return defaultValue;
        }

        private string ConvertArgumentsToJson(IDictionary<string, object> arguments)
        {
            try
            {
                if (arguments.IsNullOrEmpty())
                {
                    return "{}";
                }

                var dictionary = new Dictionary<string, object>();

                foreach (var argument in arguments)
                {
                    if (argument.Value == null)
                    {
                        dictionary[argument.Key] = null;
                    }
                    else
                    {
                        dictionary[argument.Key] = argument.Value;
                    }
                }

                return JsonUtilities.ConvertObjectToJson(dictionary);
            }
            catch
            {
                return "{}";
            }
        }

        //private static Dictionary<string, object> CreateArgumentsDictionary(MethodInfo method, object[] arguments)
        //{
        //    var parameters = method.GetParameters();
        //    var dictionary = new Dictionary<string, object>();

        //    for (var i = 0; i < parameters.Length; i++)
        //    {
        //        dictionary[parameters[i].Name] = arguments[i];
        //    }

        //    return dictionary;
        //}
    }
}