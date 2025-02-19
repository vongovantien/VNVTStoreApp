using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    /// <summary>
    /// Use for define confirm exception
    /// </summary>
    public class ConfirmationException : Exception
    {
        public ConfirmationException(string message) : base(message)
        {
        }
    }
}
