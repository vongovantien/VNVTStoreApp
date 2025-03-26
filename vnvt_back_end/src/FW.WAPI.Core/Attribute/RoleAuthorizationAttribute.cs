using System;


namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class RoleAuthorizationAttribute : System.Attribute
    {
        public string FunctionName;
        public string PermissionName;
        public bool IsSaveAuditLog;

        public RoleAuthorizationAttribute(string functionName, string permissionName,
            bool isSaveAuditLog = true)
        {
            FunctionName = functionName;
            PermissionName = permissionName;
            IsSaveAuditLog = isSaveAuditLog;
        }
    }
}
