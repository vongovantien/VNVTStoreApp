namespace VNVTStore.Application.Strategies;

/// <summary>
/// Context for building SQL conditions with all necessary information.
/// </summary>
public record ConditionContext(
    string SearchField,
    object? SearchValue,
    string? TableAlias,
    bool IsGrouped = false
);

/// <summary>
/// Strategy interface for building SQL conditions.
/// Each implementation handles one condition type (SRP).
/// </summary>
public interface IConditionStrategy
{
    /// <summary>
    /// Builds SQL condition string using StringBuilder for performance.
    /// </summary>
    string Build(ConditionContext context);
    
    /// <summary>
    /// Builds SQL condition with parameterized value (SQL injection safe).
    /// </summary>
    (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex);
}

/// <summary>
/// Base class with shared SQL building utilities (DRY).
/// </summary>
public abstract class BaseConditionStrategy : IConditionStrategy
{
    protected static string QuoteField(string? tableAlias, string field)
    {
        return string.IsNullOrEmpty(tableAlias)
            ? $"\"{field}\""
            : $"\"{tableAlias}\".\"{field}\"";
    }
    
    protected static string NormalizeFieldName(string field)
    {
        if (string.IsNullOrEmpty(field)) return field;
        return char.IsLower(field[0]) 
            ? char.ToUpper(field[0]) + field.Substring(1) 
            : field;
    }

    public abstract string Build(ConditionContext context);
    
    public virtual (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex)
    {
        // Default: fallback to non-parameterized (override for safe implementation)
        return (Build(context), new Dictionary<string, object>());
    }
}
