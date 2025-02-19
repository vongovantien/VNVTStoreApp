using FW.WAPI.Core.Attribute;
using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.Extension;
using FW.WAPI.Core.Filter;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Runtime.Audit
{
    public class LogStashFilter : IAsyncActionFilter
    {
        private readonly IStartupCoreOptions _startupCoreOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IClientInfoProvider _httpContextClientInfoProvider;
        private readonly IBaseSession _baseSession;
        private readonly IServiceProvider _serviceProvider;

        public LogStashFilter(IStartupCoreOptions startupCoreOptions, IConfiguration configuration,
            ILogger<LogStashFilter> logger, IClientInfoProvider httpContextClientInfoProvider, IBaseSession baseSession,
            IServiceProvider serviceProvider)
        {
            _startupCoreOptions = startupCoreOptions;
            _logger = logger;
            _httpContextClientInfoProvider = httpContextClientInfoProvider;
            _baseSession = baseSession;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!ShouldSaveAudit(context))
            {
                await next();
                return;
            }

            //get parameters of action
            var paras = JsonUtilities.ConvertArgumentsToJson(context.ActionArguments);
            var paramsProcessed = JsonUtilities.ReplaceSensitiveValue(paras);
            var serviceName = _configuration.GetSection("ServiceName").Value;
            serviceName = string.IsNullOrEmpty(serviceName) ? Guid.NewGuid().ToString().ToLower()
                : serviceName;

            var auditInfo = new AuditLogInfo
            {
                BrowserInfo = _httpContextClientInfoProvider.BrowserInfo,
                ClientIpAddress = _httpContextClientInfoProvider.ClientIpAddress,
                ClientName = _httpContextClientInfoProvider.ComputerName,
                MethodName = context.ActionDescriptor.AsControllerActionDescriptor().MethodInfo.Name,
                Parameters = paramsProcessed,
                ExecutionTime = DateTime.Now,
                UserId = _baseSession.UserName,
                TenantId = _baseSession.TenantRequest,
                ServiceName = serviceName
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await next();

                if (result.Exception != null && !result.ExceptionHandled)
                {
                    auditInfo.Exception = result.Exception.Message.ToString();
                }
                else if (result.Result is OkObjectResult responseResult)
                {
                    if (responseResult.Value != null && 
                        responseResult.Value.GetType().Equals(typeof(ResponseDTO)))
                    {
                        var responseValue = (ResponseDTO)responseResult.Value;

                        auditInfo.ResponseCode = responseValue?.Code;
                        auditInfo.ResponseMessage = responseValue?.Message;
                    }

                    auditInfo.ResponseCode = (int)HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                auditInfo.Exception = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                auditInfo.ExecutionDuration = Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds);
                // await _auditLogResolver.SaveAsync(auditInfo);
                _logger.Log(_startupCoreOptions.LogLevel, (JsonUtilities.ConvertObjectToJson(auditInfo)));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ShouldSaveAudit(ActionExecutingContext context)
        {
            if (!_startupCoreOptions.IsAuditing)
                return false;

            if (!(context.ActionDescriptor is ControllerActionDescriptor))
                return false;

            var controllerDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var methodInfo = controllerDescriptor.MethodInfo;

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

            return true;
        }
    }
}