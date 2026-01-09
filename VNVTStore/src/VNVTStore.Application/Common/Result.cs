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
        new("NotFound", MessageConstants.Get(MessageConstants.EntityNotFound, entityName, identifier));
    
    public static Error NotFound(string? message = null) => 
        new("NotFound", message ?? MessageConstants.Get(MessageConstants.NotFound));
    
    public static Error Validation(string message) => 
        new("Validation", message);
    
    public static Error Validation(string key, params object[] args) => 
        new("Validation", MessageConstants.Get(key, args));
    
    public static Error Conflict(string message) => 
        new("Conflict", message);

    public static Error Conflict(string key, params object[] args) => 
        new("Conflict", MessageConstants.Get(key, args));
    
    public static Error Unauthorized(string? message = null) => 
        new("Unauthorized", message ?? MessageConstants.Get(MessageConstants.Unauthorized));
    
    public static Error Forbidden(string? message = null) => 
        new("Forbidden", message ?? MessageConstants.Get(MessageConstants.Forbidden));
}
