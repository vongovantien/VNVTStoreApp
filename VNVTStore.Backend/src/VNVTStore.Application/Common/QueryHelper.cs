using System.Linq.Expressions;
using VNVTStore.Application.DTOs;
using System.Reflection;

namespace VNVTStore.Application.Common;

public static class QueryHelper
{
    public static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<SearchDTO>? filters)
    {
        if (filters == null || !filters.Any())
            return query;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field) || string.IsNullOrWhiteSpace(filter.Value))
                continue;

            // Simple property access (case-insensitive)
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyName = GetPropertyName(typeof(T), filter.Field);
            
            if (propertyName == null) continue; // Property not found

            var property = Expression.Property(parameter, propertyName);
            var constant = Expression.Constant(ConvertValue(filter.Value, property.Type));
            
            Expression? comparison = null;

            switch (filter.Operator)
            {
                case SearchCondition.Equal:
                    comparison = Expression.Equal(property, constant);
                    break;
                case SearchCondition.NotEqual:
                    comparison = Expression.NotEqual(property, constant);
                    break;
                case SearchCondition.Contains:
                    if (property.Type == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        comparison = Expression.Call(property, method!, constant);
                    }
                    else
                    {
                         // Fallback for non-string contains (e.g. strict equality)
                         comparison = Expression.Equal(property, constant);
                    }
                    break;
                case SearchCondition.GreaterThan:
                    comparison = Expression.GreaterThan(property, constant);
                    break;
                case SearchCondition.GreaterThanEqual:
                    comparison = Expression.GreaterThanOrEqual(property, constant);
                    break;
                case SearchCondition.LessThan:
                    comparison = Expression.LessThan(property, constant);
                    break;
                case SearchCondition.LessThanEqual:
                    comparison = Expression.LessThanOrEqual(property, constant);
                    break;
                // Add other cases as needed
                default:
                    comparison = Expression.Equal(property, constant);
                    break;
            }

            if (comparison != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
                query = query.Where(lambda);
            }
        }

        return query;
    }

    private static string? GetPropertyName(Type type, string fieldName)
    {
        var property = type.GetProperties()
            .FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        return property?.Name;
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(Guid)) return Guid.Parse(value);
        
        // Handle Nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (string.IsNullOrWhiteSpace(value)) return null!;
            return Convert.ChangeType(value, underlyingType);
        }

        return Convert.ChangeType(value, targetType);
    }
}
