using FW.WAPI.Core.Attribute;
using FW.WAPI.Core.DAL.Model.Audit;
using FW.WAPI.Core.General;
using FW.WAPI.Core.Runtime.Session;
using FW.WAPI.Core.Service.Remote;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Filter
{
    public class RoleAuthorizationFilter : IAsyncActionFilter
    {
        private const string CorrelationHeader = "X-Correlation-ID";
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBaseSession _baseSession;

        public RoleAuthorizationFilter(IBaseSession baseSession, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _baseSession = baseSession;
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var controllerDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var methodInfo = controllerDescriptor.MethodInfo;

                if (methodInfo.IsDefined(typeof(RoleAuthorizationAttribute), false))
                {
                    if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(CorrelationHeader))
                    {
                        await next();
                        return;
                    }

                    //Check header tenantCode
                    if (!string.IsNullOrEmpty(_baseSession.TenantCode) && !string.IsNullOrEmpty(_baseSession.TenantRequest))
                    {
                        var tenantRequest = _baseSession.TenantRequest;
                        var tenants = tenantRequest.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (!tenants.Contains(_baseSession.TenantCode))
                        {
                            context.Result = new ForbidResult();
                            return;
                        }
                    }

                    var rolAuthorizeAttribute = methodInfo.GetCustomAttribute<RoleAuthorizationAttribute>();
                    var functionsPermissions = rolAuthorizeAttribute.FunctionName.Trim();
                    var permissionName = rolAuthorizeAttribute.PermissionName.Trim();

                    //Split string perrmision and trim
                    var arrPermissionNames = permissionName.Split(",",
                        StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    var arrFunctionsPermissions = functionsPermissions.Split(",",
                        StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                    string[] roleCodes = _baseSession.Roles;
                    if (!string.IsNullOrEmpty(_baseSession.RoleCode))
                    {
                        roleCodes = new string[] { _baseSession.RoleCode };
                    }

                    bool checkPermission;
                    if (arrFunctionsPermissions.Length > 1)
                    {
                        checkPermission = await _authorizationService.CheckAuthorization(roleCodes, arrFunctionsPermissions, arrPermissionNames);
                    }
                    else
                    {
                        checkPermission = await _authorizationService.CheckAuthorization(roleCodes, functionsPermissions, permissionName);
                    }

                    //Check permision
                    if (checkPermission)
                    {
                        await next();

                        try
                        {
                            // Check action need to log activity
                            if (rolAuthorizeAttribute.IsSaveAuditLog
                                && arrPermissionNames.Count() == 1
                                && arrFunctionsPermissions.Count() == 1)
                            {
                                string functionCode = arrFunctionsPermissions.First().Trim();
                                string permissionCode = arrPermissionNames.First().Trim();
                                object actionValue;
                                Enum.TryParse(typeof(EnumTypes.AuditLogAction), permissionCode, out actionValue);
                                if (actionValue == null) return;

                                var auditLogData = new AuditLog
                                {
                                    UserCode = _baseSession.UserName,
                                    Parameters = JsonUtilities.ConvertArgumentsToJson(context.ActionArguments),
                                    FunctionCode = functionCode,
                                    ActionCode = (int)actionValue,
                                    ReceivedTime = DateTime.Now,
                                    TenantCode = _baseSession.TenantCode
                                };
                                context.HttpContext.Items.Add(AuditLogConst.AUDIT_LOG_DATA_KEY, auditLogData);
                            }
                        }
                        catch { }

                        return;
                    }
                    else
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }

                await next();
            }
            catch
            {
                await next();
            }
        }
    }
}