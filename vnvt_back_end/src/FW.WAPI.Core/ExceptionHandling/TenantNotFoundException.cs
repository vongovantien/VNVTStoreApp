using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    public class TenantNotFoundException : Exception
    {
        public TenantNotFoundException(string message) : base(message)
        {
        }
    }
}