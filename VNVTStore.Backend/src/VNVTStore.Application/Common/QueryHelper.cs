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

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field) || filter.Value == null)
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
                case SearchCondition.In:
                case SearchCondition.NotIn:
                    try
                    {
                        List<string>? stringValues = null;

                        if (filter.Value is JsonElement valueElement && valueElement.ValueKind == JsonValueKind.Array)
                        {
                            stringValues = valueElement.EnumerateArray()
                                .Select(v => v.ToString())
                                .ToList();
                        }
                        else if (filter.Value is IEnumerable<string> listStr)
                        {
                            stringValues = listStr.ToList();
                        }
                        else if (filter.Value is string strVal)
                        {
                             // Fallback for legacy stringified JSON
                             try { stringValues = JsonSerializer.Deserialize<List<string>>(strVal); } catch {}
                        }

                        if (stringValues != null && stringValues.Any())
                        {
                            var methodInfo = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                            var listExpr = Expression.Constant(stringValues);
                            comparison = Expression.Call(listExpr, methodInfo!, property);
                            
                            if (filter.Operator == SearchCondition.NotIn)
                            {
                                comparison = Expression.Not(comparison);
                            }
                        }
                    }
                    catch { } 
                    break;
                case SearchCondition.IsNull:
                     comparison = Expression.Equal(property, Expression.Constant(null));
                     break;
                case SearchCondition.IsNotNull:
                     comparison = Expression.NotEqual(property, Expression.Constant(null));
                     break;
                case SearchCondition.EqualExact:
                     if (property.Type == typeof(string))
                     {
                         var method = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                         comparison = Expression.Call(property, method!, constant);
                     }
                     else
                     {
                         comparison = Expression.Equal(property, constant);
                     }
                     break;
                case SearchCondition.DateTimeRange:
                     try 
                     {
                         List<string>? dates = null;
                         if (filter.Value is JsonElement dateElement && dateElement.ValueKind == JsonValueKind.Array)
                         {
                             dates = dateElement.EnumerateArray().Select(v => v.ToString()).ToList();
                         }
                         else if (filter.Value is IEnumerable<string> listStr)
                         {
                             dates = listStr.ToList();
                         }
                         else if (filter.Value is string strVal)
                         {
                             try { dates = JsonSerializer.Deserialize<List<string>>(strVal); } catch {}
                         }

                         if (dates != null && dates.Count >= 2 && 
                             DateTime.TryParse(dates[0], out var start) && 
                             DateTime.TryParse(dates[1], out var end))
                         {
                             var startExpr = Expression.GreaterThanOrEqual(property, Expression.Constant(start));
                             var endExpr = Expression.LessThanOrEqual(property, Expression.Constant(end));
                             comparison = Expression.AndAlso(startExpr, endExpr);
                         }
                     }
                     catch { }
                     break;
                case SearchCondition.DayPart:
                case SearchCondition.MonthPart:
                case SearchCondition.DatePart:
                     // Handle date parts
                     if (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?))
                     {
                         Expression targetProp = property;
                         if (property.Type == typeof(DateTime?))
                         {
                             targetProp = Expression.Property(property, "Value");
                         }
                         
                         string valStr = filter.Value?.ToString() ?? "";
                         if (filter.Value is JsonElement je && je.ValueKind != JsonValueKind.Null)
                             valStr = je.ToString();

                         if (filter.Operator == SearchCondition.DayPart && int.TryParse(valStr, out var day))
                         {
                             var dayProp = Expression.Property(targetProp, "Day");
                             comparison = Expression.Equal(dayProp, Expression.Constant(day));
                         }
                         else if (filter.Operator == SearchCondition.MonthPart && int.TryParse(valStr, out var month))
                         {
                             var monthProp = Expression.Property(targetProp, "Month");
                             comparison = Expression.Equal(monthProp, Expression.Constant(month));
                         }
                         else if (filter.Operator == SearchCondition.DatePart && DateTime.TryParse(valStr, out var date))
                         {
                             var dateProp = Expression.Property(targetProp, "Date");
                             comparison = Expression.Equal(dateProp, Expression.Constant(date.Date));
                         }
                         
                         if (property.Type == typeof(DateTime?) && comparison != null)
                         {
                             var hasValue = Expression.Property(property, "HasValue");
                             comparison = Expression.AndAlso(hasValue, comparison);
                         }
                     }
                     break;
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
