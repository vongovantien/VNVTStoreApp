

using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : System.Attribute
    {
        public bool CompanyCode { get; set; }

        public UniqueAttribute(bool companyCode = true)
        {
            CompanyCode = companyCode;
        }
    }
}
