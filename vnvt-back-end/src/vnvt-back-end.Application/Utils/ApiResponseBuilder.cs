using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Utils
{
    public static class ApiResponseBuilder
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Operation succeeded")
        {
            return new ApiResponse<T>(true, message, data, 200);
        }

        public static ApiResponse<T> Created<T>(T data, string message = "Resource created")
        {
            return new ApiResponse<T>(true, message, data, 201);
        }

        public static ApiResponse<T> Error<T>(string message, int statusCode = 400)
        {
            return new ApiResponse<T>(false, message, default, statusCode);
        }

        public static ApiResponse<T> NotFound<T>(string message = "Resource not found")
        {
            return new ApiResponse<T>(false, message, default, 404);
        }
    }
}
