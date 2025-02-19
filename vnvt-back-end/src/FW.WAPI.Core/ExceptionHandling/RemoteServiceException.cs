using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    public class RemoteServiceException : Exception
    {
        public RemoteServiceException(string message, int code) : base(message)
        {

        }
    }
}
