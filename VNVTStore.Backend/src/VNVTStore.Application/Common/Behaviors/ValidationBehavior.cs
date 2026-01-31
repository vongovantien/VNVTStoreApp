using FluentValidation;
using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior cho validation
/// Tự động validate tất cả requests trước khi xử lý
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            // Return validation error using Result pattern
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            
            // Check if TResponse is Result or Result<T>
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(Error.Validation("Validation", errorMessage));
            }
            
            // Handle Result<T> responses
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == "Failure" && m.IsGenericMethod)
                    .MakeGenericMethod(innerType);
                
                return (TResponse)failureMethod.Invoke(null, new object[] { Error.Validation("Validation", errorMessage) })!;
            }
            
            // Fallback: throw exception if response type is not Result
            throw new ValidationException(failures);
        }

        return await next();
    }
}
