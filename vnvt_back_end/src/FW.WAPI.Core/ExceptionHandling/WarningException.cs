using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    /// <summary>
    /// Use for define warning exception
    /// </summary>
    public class WarningException : Exception
    {
        public WarningException(string message) : base(message)
        {
        }
    }
}
