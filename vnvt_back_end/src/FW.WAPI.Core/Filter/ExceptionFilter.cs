using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace FW.WAPI.Core.Filter
{
    public class ExceptionFilter : IFilterMetadata
    {
        public void OnException(ExceptionContext context)
        {
            var exceptionType = context.Exception.GetType();
            string message;
            HttpStatusCode status;
            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                message = "Unauthorized Access";
                status = HttpStatusCode.Unauthorized;
            }
            else if (exceptionType == typeof(NotImplementedException))
            {
                message = "A server error occurred.";
                status = HttpStatusCode.NotImplemented;
            }
            else
            {
                message = context.Exception.Message;
                status = HttpStatusCode.NotFound;
            }

            HttpResponse response = context.HttpContext.Response;

            response.StatusCode = (int)status;
            response.ContentType = "application/json";
            var err = message + " " + context.Exception.StackTrace;
            response.WriteAsync(err);
        }
    }
}