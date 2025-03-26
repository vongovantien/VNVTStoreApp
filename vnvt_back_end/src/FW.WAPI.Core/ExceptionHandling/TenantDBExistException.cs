using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public class TenantDBExistException : Exception
    {
        public TenantDBExistException(string message) : base(message)
        {

        }
    }
}
