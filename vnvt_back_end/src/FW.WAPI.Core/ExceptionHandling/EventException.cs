using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    public class EventException : Exception
    {
        public EventException(string message) : base(message)
        {
        }
    }
}