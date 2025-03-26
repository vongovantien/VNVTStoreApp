
using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : System.Attribute
    {
        public bool CompanyCode { get; set; }

        public PrimaryKeyAttribute(bool companyCode = true)
        {
            CompanyCode = companyCode;
        }
    }
}
