using System.Linq.Expressions;
using VNVTStore.Application.DTOs;
using System.Reflection;
using System.Text.Json;

namespace VNVTStore.Application.Common;

public static class QueryHelper
{
    public static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<SearchDTO>? filters)
    {
        if (filters == null || !filters.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        // 1. Handle non-grouped filters (Global AND) - GroupID == null
        var globalFilters = filters.Where(f => f.GroupID == null).ToList();
        foreach (var filter in globalFilters)
        {
            var comparison = BuildComparison(filter, parameter);
            if (comparison != null)
            {
                query = query.Where(Expression.Lambda<Func<T, bool>>(comparison, parameter));
            }
        }

        // 2. Handle grouped filters (e.g. (A OR B) AND (C OR D))
        var groups = filters.Where(f => f.GroupID != null).GroupBy(f => f.GroupID);
        foreach (var group in groups)
        {
            Expression? groupExpression = null;
            var combineCondition = group.First().CombineCondition?.ToUpper() ?? "AND"; // Default to AND if not set, but groups usually imply OR or specific logic

            foreach (var filter in group)
            {
                var comparison = BuildComparison(filter, parameter);
                if (comparison == null) continue;

                if (groupExpression == null)
                {
                    groupExpression = comparison;
                }
                else
                {
                    if (combineCondition == "OR")
                    {
                        groupExpression = Expression.OrElse(groupExpression, comparison);
                    }
                    else
                    {
                        groupExpression = Expression.AndAlso(groupExpression, comparison);
                    }
                }
            }

            if (groupExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<T, bool>>(groupExpression, parameter));
            }
        }

        return query;
    }

    private static Expression? BuildComparison(SearchDTO filter, ParameterExpression parameter)
    {
        if (string.IsNullOrWhiteSpace(filter.SearchField) || 
            (filter.SearchValue == null && 
             filter.SearchCondition != SearchCondition.IsNull && 
             filter.SearchCondition != SearchCondition.IsNotNull))
            return null;

        var propertyName = GetPropertyName(parameter.Type, filter.SearchField);
        if (propertyName == null) return null;

        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(ConvertValue(filter.SearchValue, property.Type));

        switch (filter.SearchCondition)
        {
            case SearchCondition.Equal:
                return Expression.Equal(property, constant);
            case SearchCondition.NotEqual:
                return Expression.NotEqual(property, constant);
            case SearchCondition.Contains:
                if (property.Type == typeof(string))
                {
                    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    return Expression.Call(property, method!, constant);
                }
                return Expression.Equal(property, constant);
            case SearchCondition.GreaterThan:
                return Expression.GreaterThan(property, constant);
            case SearchCondition.GreaterThanEqual:
                return Expression.GreaterThanOrEqual(property, constant);
            case SearchCondition.LessThan:
                return Expression.LessThan(property, constant);
            case SearchCondition.LessThanEqual:
                return Expression.LessThanOrEqual(property, constant);
            case SearchCondition.In:
            case SearchCondition.NotIn:
                try
                {
                    List<string>? stringValues = null;

                    if (filter.SearchValue is JsonElement valueElement && valueElement.ValueKind == JsonValueKind.Array)
                    {
                        stringValues = valueElement.EnumerateArray()
                            .Select(v => v.ToString())
                            .ToList();
                    }
                    else if (filter.SearchValue is IEnumerable<string> listStr)
                    {
                        stringValues = listStr.ToList();
                    }
                    else if (filter.SearchValue is string strVal)
                    {
                        try { stringValues = JsonSerializer.Deserialize<List<string>>(strVal); } catch { }
                    }

                    if (stringValues != null && stringValues.Any())
                    {
                        var methodInfo = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                        var listExpr = Expression.Constant(stringValues);
                        var call = Expression.Call(listExpr, methodInfo!, property);

                        return filter.SearchCondition == SearchCondition.NotIn ? Expression.Not(call) : call;
                    }
                }
                catch { }
                return null;
            case SearchCondition.IsNull:
                return Expression.Equal(property, Expression.Constant(null));
            case SearchCondition.IsNotNull:
                return Expression.NotEqual(property, Expression.Constant(null));
            case SearchCondition.EqualExact:
                if (property.Type == typeof(string))
                {
                    var method = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                    return Expression.Call(property, method!, constant);
                }
                return Expression.Equal(property, constant);
            case SearchCondition.DateTimeRange:
                try
                {
                    List<string>? dates = null;
                    if (filter.SearchValue is JsonElement dateElement && dateElement.ValueKind == JsonValueKind.Array)
                    {
                        dates = dateElement.EnumerateArray().Select(v => v.ToString()).ToList();
                    }
                    else if (filter.SearchValue is IEnumerable<string> listStr)
                    {
                        dates = listStr.ToList();
                    }
                    else if (filter.SearchValue is string strVal)
                    {
                        try { dates = JsonSerializer.Deserialize<List<string>>(strVal); } catch { }
                    }

                    if (dates != null && dates.Count >= 2 &&
                        DateTime.TryParse(dates[0], out var start) &&
                        DateTime.TryParse(dates[1], out var end))
                    {
                        var startExpr = Expression.GreaterThanOrEqual(property, Expression.Constant(start));
                        var endExpr = Expression.LessThanOrEqual(property, Expression.Constant(end));
                        return Expression.AndAlso(startExpr, endExpr);
                    }
                }
                catch { }
                return null;
            case SearchCondition.DayPart:
            case SearchCondition.MonthPart:
            case SearchCondition.DatePart:
                if (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?))
                {
                    Expression targetProp = property;
                    if (property.Type == typeof(DateTime?))
                    {
                        targetProp = Expression.Property(property, "Value");
                    }

                    string valStr = filter.SearchValue?.ToString() ?? "";
                    if (filter.SearchValue is JsonElement je && je.ValueKind != JsonValueKind.Null)
                        valStr = je.ToString();

                    Expression? comparison = null;
                    if (filter.SearchCondition == SearchCondition.DayPart && int.TryParse(valStr, out var day))
                    {
                        var dayProp = Expression.Property(targetProp, "Day");
                        comparison = Expression.Equal(dayProp, Expression.Constant(day));
                    }
                    else if (filter.SearchCondition == SearchCondition.MonthPart && int.TryParse(valStr, out var month))
                    {
                        var monthProp = Expression.Property(targetProp, "Month");
                        comparison = Expression.Equal(monthProp, Expression.Constant(month));
                    }
                    else if (filter.SearchCondition == SearchCondition.DatePart && DateTime.TryParse(valStr, out var date))
                    {
                        var dateProp = Expression.Property(targetProp, "Date");
                        comparison = Expression.Equal(dateProp, Expression.Constant(date.Date));
                    }

                    if (property.Type == typeof(DateTime?) && comparison != null)
                    {
                        var hasValue = Expression.Property(property, "HasValue");
                        return Expression.AndAlso(hasValue, comparison);
                    }
                    return comparison;
                }
                return null;
            default:
                return Expression.Equal(property, constant);
        }
    }

    private static string? GetPropertyName(Type type, string fieldName)
    {
        var property = type.GetProperties()
            .FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        return property?.Name;
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null) return null!;

        // Handle JsonElement
        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return ConvertStringValue(element.GetString()!, targetType);
            if (element.ValueKind == JsonValueKind.Number)
            {
                 // Handle numbers
                 if (targetType == typeof(int) || targetType == typeof(int?)) return element.GetInt32();
                 if (targetType == typeof(long) || targetType == typeof(long?)) return element.GetInt64();
                 if (targetType == typeof(double) || targetType == typeof(double?)) return element.GetDouble();
                 if (targetType == typeof(decimal) || targetType == typeof(decimal?)) return element.GetDecimal();
                 if (targetType == typeof(float) || targetType == typeof(float?)) return (float)element.GetDouble();
            }
            if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
                return element.GetBoolean();
                
            return ConvertStringValue(element.ToString(), targetType);
        }

        return ConvertStringValue(value.ToString()!, targetType);
    }

    private static object ConvertStringValue(string value, Type targetType)
    {
        if (targetType == typeof(Guid)) return Guid.Parse(value);
        
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (string.IsNullOrWhiteSpace(value)) return null!;
            return Convert.ChangeType(value, underlyingType);
        }

        return Convert.ChangeType(value, targetType);
    }

    public static IQueryable<T> ApplySelection<T>(IQueryable<T> query, List<string>? fields)
    {
        if (fields == null || !fields.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var bindings = new List<MemberBinding>();
        var type = typeof(T);

        // Group fields to handle nested objects/collections
        // Format: "Name", "Category.Name", "Questions.Content"
        var fieldGroups = fields.Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .Select(f => f.Split(new[] { '.' }, 2))
            .GroupBy(parts => parts[0], StringComparer.OrdinalIgnoreCase);

        foreach (var group in fieldGroups)
        {
            var propertyName = group.Key;
            var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            if (property == null) continue;

            // Simple property (no dot) or explicitly requested
            var subFields = group.Where(parts => parts.Length > 1).Select(parts => parts[1]).ToList();
            
            if (!subFields.Any())
            {
                // Select entire property
                bindings.Add(Expression.Bind(property, Expression.Property(parameter, property)));
            }
            else
            {
                // Nested selection
                var propertyAccess = Expression.Property(parameter, property);
                
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                     // Collection: x.Items.Select(i => new Item { ... })
                     // Find generic argument
                     var itemType = property.PropertyType.IsGenericType 
                        ? property.PropertyType.GetGenericArguments()[0] 
                        : property.PropertyType.GetElementType();

                     if (itemType != null)
                     {
                         var subParameter = Expression.Parameter(itemType, "sub");
                         var subBindings = GetMemberBindings(itemType, subParameter, subFields);
                         var subInit = Expression.MemberInit(Expression.New(itemType), subBindings);
                         var subSelectLambda = Expression.Lambda(subInit, subParameter);
                         
                         var selectMethod = typeof(Enumerable).GetMethods()
                             .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                             .MakeGenericMethod(itemType, itemType);

                         var selectCall = Expression.Call(selectMethod, propertyAccess, subSelectLambda);
                         
                         // Determine target type (List, ICollection, etc.)
                         // EF often needs ToList() for collections in projection if property is List
                         // But for IQueryable projection, .ToList() inside Select is supported if target is List
                         var toListMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(itemType);
                         var toListCall = Expression.Call(toListMethod, selectCall);

                         bindings.Add(Expression.Bind(property, toListCall));
                     }
                }
                else
                {
                    // Nested Object: x.Category -> new Category { Name = ... }
                    var subBindings = GetMemberBindings(property.PropertyType, propertyAccess, subFields);
                    var subInit = Expression.MemberInit(Expression.New(property.PropertyType), subBindings);
                    bindings.Add(Expression.Bind(property, subInit));
                }
            }
        }

        if (!bindings.Any()) return query;

        var selector = Expression.MemberInit(Expression.New(type), bindings);
        var lambda = Expression.Lambda<Func<T, T>>(selector, parameter);

        return query.Select(lambda);
    }

    private static List<MemberBinding> GetMemberBindings(Type type, Expression parameter, List<string> fields)
    {
        var bindings = new List<MemberBinding>();
        var fieldGroups = fields.Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .Select(f => f.Split(new[] { '.' }, 2))
            .GroupBy(parts => parts[0], StringComparer.OrdinalIgnoreCase);

        foreach (var group in fieldGroups)
        {
             var propertyName = group.Key;
             var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
             if (property == null) continue;

             var subFields = group.Where(parts => parts.Length > 1).Select(parts => parts[1]).ToList();
             
             if (!subFields.Any())
             {
                 bindings.Add(Expression.Bind(property, Expression.Property(parameter, property)));
             }
             else
             {
                 var propertyAccess = Expression.Property(parameter, property);
                 // Recursive handle for deeper nesting if needed (simplified for 2 levels here or recursion)
                 // Re-use logic for recursion?
                 // For now, assume simple 1-level deep in this helper for brevity or duplicate logic for robustness
                 // Let's recurse if object
                 if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) || property.PropertyType == typeof(string))
                 {
                      var propInit = Expression.MemberInit(Expression.New(property.PropertyType), GetMemberBindings(property.PropertyType, propertyAccess, subFields));
                      bindings.Add(Expression.Bind(property, propInit));
                 }
             }
        }
        return bindings;
    }
}
