using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    public class PropertyNotExistException : Exception
    {
        public PropertyNotExistException(string massage = "Property is not exist") : base(massage)
        {

        }
    }
}
