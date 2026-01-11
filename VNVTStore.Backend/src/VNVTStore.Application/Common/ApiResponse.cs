namespace VNVTStore.Application.Common;

/// <summary>
/// API Response wrapper - dùng chung cho tất cả API responses
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message ?? MessageConstants.Get(MessageConstants.Success),
        Data = data,
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Message = message ?? MessageConstants.Get(MessageConstants.Created),
        Data = data,
        StatusCode = 201
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        StatusCode = statusCode
    };

    public static ApiResponse<T> NotFound(string? message = null) => new()
    {
        Success = false,
        Message = message ?? MessageConstants.Get(MessageConstants.NotFound),
        Data = default,
        StatusCode = 404
    };

    public static ApiResponse<T> Unauthorized(string? message = null) => new()
    {
        Success = false,
        Message = message ?? MessageConstants.Get(MessageConstants.Unauthorized),
        Data = default,
        StatusCode = 401
    };

    public static ApiResponse<T> Forbidden(string? message = null) => new()
    {
        Success = false,
        Message = message ?? MessageConstants.Get(MessageConstants.Forbidden),
        Data = default,
        StatusCode = 403
    };
}
