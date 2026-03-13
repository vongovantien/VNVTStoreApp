using MediatR;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common.Interfaces;
using VNVTStore.Application.Interfaces;
using System.Reflection;

namespace VNVTStore.Application.Common.Behaviors;

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(IAuditLogService auditLogService, ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Only log if the request is auditable
        if (request is IAuditableCommand auditableRequest)
        {
            // Check if result is successful (Result or Result<T>)
            bool isSuccess = false;
            if (response is Result result)
                isSuccess = result.IsSuccess;
            else if (response != null && response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(Result<>))
            {
                 var isSuccessProp = response.GetType().GetProperty("IsSuccess");
                 if (isSuccessProp != null)
                     isSuccess = (bool)isSuccessProp.GetValue(response)!;
            }

            if (isSuccess)
            {
                try
                {
                    string action = auditableRequest.AuditAction ?? GetDefaultActionName(request);
                    string? resourceId = auditableRequest.AuditResourceId ?? GetDefaultResourceId(request);

                    await _auditLogService.LogAsync(action, resourceId);
                    _logger.LogInformation("Audit Logged: {Action} for Resource: {ResourceId}", action, resourceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while logging audit for {RequestName}", typeof(TRequest).Name);
                }
            }
        }

        return response;
    }

    private string GetDefaultActionName(TRequest request)
    {
        var name = request.GetType().Name;
        
        // Remove 'Command' suffix if present
        if (name.EndsWith("Command"))
            name = name.Substring(0, name.Length - 7);
            
        // Convert to UPPER_SNAKE_CASE (rough approximation)
        return ToUpperSnakeCase(name);
    }

    private string? GetDefaultResourceId(TRequest request)
    {
        // Try to find a property named 'Code' or 'Id' or 'UserName' via reflection
        var type = request.GetType();
        var props = type.GetProperties();
        
        var codeProp = props.FirstOrDefault(p => p.Name.Equals("Code", StringComparison.OrdinalIgnoreCase) 
                                              || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                                              || p.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase));
                                              
        return codeProp?.GetValue(request)?.ToString();
    }

    private string ToUpperSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i]))
            {
                sb.Append('_');
            }
            sb.Append(char.ToUpper(input[i]));
        }
        return sb.ToString();
    }
}
