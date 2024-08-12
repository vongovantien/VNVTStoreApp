using vnvt_back_end.Application.Models;

namespace vnvt_back_end.Application.Utils
{
    public static class ApiResponseBuilder
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Operation succeeded")
        {
            return new ApiResponse<T>(true, message, data, 200);
        }

        public static ApiResponse<T> Created<T>(T data, string message = "Resource created successfully")
        {
            return new ApiResponse<T>(true, message, data, 201);
        }

        public static ApiResponse<T> BadRequest<T>(string message = "Bad request")
        {
            return new ApiResponse<T>(false, message, default, 400);
        }

        public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized access")
        {
            return new ApiResponse<T>(false, message, default, 401);
        }

        public static ApiResponse<T> NotFound<T>(string message = "Resource not found")
        {
            return new ApiResponse<T>(false, message, default, 404);
        }

        public static ApiResponse<T> InternalServerError<T>(string message = "An unexpected error occurred")
        {
            return new ApiResponse<T>(false, message, default, 500);
        }

        public static ApiResponse<T> ValidationError<T>(string message = "Validation failed")
        {
            return new ApiResponse<T>(false, message, default, 422);
        }

        public static ApiResponse<T> CustomError<T>(string message, int statusCode)
        {
            return new ApiResponse<T>(false, message, default, statusCode);
        }
    }
}
