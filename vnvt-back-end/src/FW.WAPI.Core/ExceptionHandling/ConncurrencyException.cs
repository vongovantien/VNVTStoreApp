using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message)
        {
        }
    }
}
