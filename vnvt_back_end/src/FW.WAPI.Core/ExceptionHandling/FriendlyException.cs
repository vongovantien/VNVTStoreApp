using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    /// <summary>
    /// Use for define friendly exception
    /// </summary>
    public class FriendlyException : Exception
    {
        public FriendlyException(string message) : base(message)
        {
        }
    }
}
