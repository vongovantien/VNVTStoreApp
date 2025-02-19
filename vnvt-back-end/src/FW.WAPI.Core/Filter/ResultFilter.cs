using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.DAL.Model.Configuration;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Infrastructure.Logger;
using FW.WAPI.Core.Service.Audit;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Filter
{
    public class ResultFilter : IAsyncResultFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ResultCodeTable _resultCode;
        public ResultFilter(IServiceProvider serviceProvider, IOptions<ResultCodeTable> resultCode)
        {
            _serviceProvider = serviceProvider;
            _resultCode = resultCode.Value;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Check request has expired token from remote service
            var headerAuthen = context.HttpContext.Response.Headers[HeaderNames.WWWAuthenticate].ToString();
            if (!string.IsNullOrEmpty(headerAuthen))
            {
                context.Result = new UnauthorizedResult();
            }

            // Process Log Activity
            await ProcessLogActivity(context);

            await next();
        }

        /// <summary>
        /// Process log activity
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessLogActivity(ResultExecutingContext context)
        {
            try
            {
                if (context.Result is OkObjectResult)
                {
                    object auditLogData = null;
                    context.HttpContext.Items.TryGetValue(AuditLogConst.AUDIT_LOG_DATA_KEY, out auditLogData);
                    if (auditLogData != null)
                    {
                        var result = (OkObjectResult)context.Result;
                        var response = (ResponseDTO)result.Value;

                        ((AuditLog)auditLogData).ResponseCode = response.Code;
                        if (response.Code != _resultCode.Ok.Code)
                        {
                            ((AuditLog)auditLogData).ResponseMessage = response.Message;
                        }
                        var auditLogService = _serviceProvider.GetRequiredService<IAuditLoggingService>();
                        await auditLogService.Log((AuditLog)auditLogData);
                    }
                }
            }
            catch (Exception ex)
            {
                var coreLogger = _serviceProvider.GetRequiredService<ICoreLogger>();
                coreLogger.Log(LogLevel.Error, $"ProcessLogActivity, ex:{ex.Message}");
            }
        }
    }
}