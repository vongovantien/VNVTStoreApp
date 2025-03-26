using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.Service.Remote;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace FW.WAPI.Core.Service.Audit
{
    public class AuditLoggingService : RemoteService, IAuditLoggingService
    {
        private readonly AuditLogConfig _auditLogConfig;
        private readonly ICoreLogger _coreLogger;
        public AuditLoggingService(HttpClient httpClient, IConfiguration configuration,
            IOptions<AuditLogConfig> auditLogConfig, ICoreLogger coreLogger)
            : base(httpClient, configuration)
        {
            _auditLogConfig = auditLogConfig.Value;
            _coreLogger = coreLogger;
        }

        /// <summary>
        /// Log actitvity
        /// </summary>
        /// <param name="auditLog"></param>
        /// <returns></returns>
        public async Task Log(AuditLog auditLog)
        {
            try
            {
                auditLog.Parameters = ReplaceSensitiveValue(auditLog.Parameters);

                var request = new RequestDTO();
                request.PostObject = auditLog;
                await Post(_auditLogConfig.HttpLogUrl, request, tenantCode: auditLog.TenantCode);
            }
            catch (Exception ex)
            {
                _coreLogger.Log(LogLevel.Error, $"LogActivity, {ex.Message}");
            }
        }

        /// <summary>
        /// Remove sensitive value
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ReplaceSensitiveValue(string parameters)
        {
            try
            {
                if (parameters == null) return parameters;

                var auditLogObj = JObject.Parse(parameters);
                foreach (var field in AuditLogConst.SENSITIVE_FIELDS)
                {
                    IEnumerable<JToken>[] sensitiveFields = new IEnumerable<JToken>[]
                    {
                        auditLogObj.SelectTokens($"$.{field}"),
                        auditLogObj.SelectTokens($"$..{field}"),
                        auditLogObj.SelectTokens($"$...{field}")
                    };

                    foreach (IEnumerable<JToken> sensitiveField in sensitiveFields)
                    {
                        if (sensitiveField?.Any() != true) continue;
                        foreach(JToken item in sensitiveField)
                        {
                            var value = item.Value<string>();
                            if (value == null) continue;
                            var hiddenValue = new string(AuditLogConst.SENSITIVE_VALUE_ALTERNATIVE,
                                AuditLogConst.SENSITIVE_VALUE_ALTERNATIVE_LENGTH);
                            item.Replace(hiddenValue);
                        }                                       
                    }
                }

                return auditLogObj.ToString(Formatting.None);
            }
            catch (JsonReaderException)
            {
                return parameters;
            }
            catch (Exception ex)
            {
                _coreLogger.Log(LogLevel.Error, $"RemoveSensitiveValue, {ex.Message}");
            }

            return parameters;
        }

    }

    public interface IAuditLoggingService
    {
        Task Log(AuditLog auditLog);
    }
}