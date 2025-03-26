using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class AuditedAttribute : System.Attribute
    {
        public bool IsEnable;

        public AuditedAttribute(bool isEnable)
        {
            IsEnable = isEnable;
        }
    }
}