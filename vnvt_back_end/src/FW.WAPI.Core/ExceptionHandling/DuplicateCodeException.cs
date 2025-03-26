using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public class DuplicateCodeException : Exception
    {       
        public DuplicateCodeException(string message) : base(message)
        {

        }
    }
}
