using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.Extension;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Runtime.Audit;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Filter
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly IStartupCoreOptions _startupCoreOptions;
        private readonly IAuditLogResolver _auditLogResolver;
        private readonly ILogger _logger;
        public AuditLogFilter(IStartupCoreOptions startupCoreOptions, IAuditLogResolver auditLogResolver,
            ILogger<AuditLogFilter> logger)
        {
            _startupCoreOptions = startupCoreOptions;
            _logger = logger;
            _auditLogResolver = auditLogResolver;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!ShouldSaveAudit(context))
            {
                await next();
                return;
            }

            var auditInfo = _auditLogResolver.CreateAuditInfo(
                    context.ActionDescriptor.AsControllerActionDescriptor().ControllerTypeInfo.AsType(),
                    context.ActionDescriptor.AsControllerActionDescriptor().MethodInfo,
                    context.ActionArguments
                );

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await next();
                if (result.Exception != null && !result.ExceptionHandled)
                {
                    auditInfo.Exception = result.Exception.Message.ToString();
                }
            }
            catch (Exception ex)
            {
                auditInfo.Exception = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                auditInfo.ExecutionDuration = Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds);
               // await _auditLogResolver.SaveAsync(auditInfo);
               _logger.LogInformation(JsonUtilities.ConvertObjectToJson(auditInfo));
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

            if (!(_auditLogResolver.ShouldSaveAudit(methodInfo, true)))
                return false;

            return true;
        }
    }
}