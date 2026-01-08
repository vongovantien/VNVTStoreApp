namespace VNVTStore.Application.Common;

/// <summary>
/// Result pattern cho error handling - dùng chung cho tất cả operations
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string message) => new(false, new Error(message));
    
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(string message) => Result<T>.Failure(new Error(message));
}

/// <summary>
/// Generic Result với value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, Error? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(Error error) => new(false, default, error);
    public static Result<T> Failure(string message) => new(false, default, new Error(message));
}

/// <summary>
/// Error class cho Result pattern
/// </summary>
public class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string message) : this("Error", message) { }
    
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error NotFound(string entityName, object identifier) => 
        new("NotFound", $"{entityName} with id/code '{identifier}' not found");
    
    public static Error NotFound(string message) => 
        new("NotFound", message);
    
    public static Error Validation(string message) => 
        new("Validation", message);
    
    public static Error Validation(string entityName, string message) => 
        new("Validation", $"{entityName}: {message}");
    
    public static Error Conflict(string message) => 
        new("Conflict", message);
    
    public static Error Unauthorized(string message = "Unauthorized") => 
        new("Unauthorized", message);
    
    public static Error Forbidden(string message = "Access denied") => 
        new("Forbidden", message);
}
