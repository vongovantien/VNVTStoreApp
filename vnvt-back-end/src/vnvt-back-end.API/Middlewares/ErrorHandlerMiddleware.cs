using System.Net;
using System.Text.Json;
using vnvt_back_end.Application.Models;

namespace vnvt_back_end.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var responseModel = new ApiResponse<string>(
                success: false,
                message: exception.Message,
                data: null,
                statusCode: (int)HttpStatusCode.InternalServerError
            );

            switch (exception)
            {
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseModel.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    responseModel.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(responseModel);
            return response.WriteAsync(result);
        }
    }
}
