using System.Text;

namespace VNVTStore.Application.Strategies;

/// <summary>
/// Strategy for NULL check conditions (IS NULL, IS NOT NULL).
/// </summary>
public class NullCheckStrategy : BaseConditionStrategy
{
    private readonly bool _isNull;
    
    public NullCheckStrategy(bool isNull = true) => _isNull = isNull;
    
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        return _isNull ? $"{field} IS NULL " : $"{field} IS NOT NULL ";
    }
}

/// <summary>
/// Strategy for CONTAINS condition (ILIKE '%value%').
/// </summary>
public class ContainsStrategy : BaseConditionStrategy
{
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        return $"{field}::text ILIKE '%{context.SearchValue}%' ";
    }
    
    public override (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var paramName = $"@p{paramIndex}";
        var sql = $"{field}::text ILIKE {paramName} ";
        var parameters = new Dictionary<string, object>
        {
            [paramName] = $"%{context.SearchValue}%"
        };
        return (sql, parameters);
    }
}

/// <summary>
/// Strategy for EQUAL condition with type handling.
/// </summary>
public class EqualStrategy : BaseConditionStrategy
{
    private readonly bool _caseSensitive;
    
    public EqualStrategy(bool caseSensitive = false) => _caseSensitive = caseSensitive;
    
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var value = context.SearchValue;
        
        if (value == null)
            return $"{field} IS NULL ";
        
        var typeName = value.GetType().FullName;
        
        return typeName switch
        {
            "System.Boolean" => $"{field} = {value} ",
            "System.DateTime" => $"{field} = to_timestamp('{((DateTime)value):dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS') ",
            "System.String" when !_caseSensitive => $"{field} ILIKE '{value}' ",
            "System.String" => $"{field} = '{value}' ",
            _ when double.TryParse(value.ToString(), out _) => $"{field} = {value} ",
            _ => $"{field} = '{value}' "
        };
    }
    
    public override (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var paramName = $"@p{paramIndex}";
        
        if (context.SearchValue == null)
            return ($"{field} IS NULL ", new Dictionary<string, object>());
        
        var sql = _caseSensitive 
            ? $"{field} = {paramName} "
            : $"{field} ILIKE {paramName} ";
            
        return (sql, new Dictionary<string, object> { [paramName] = context.SearchValue });
    }
}

/// <summary>
/// Strategy for comparison conditions (>, <, >=, <=).
/// </summary>
public class ComparisonStrategy : BaseConditionStrategy
{
    private readonly string _operator;
    
    public ComparisonStrategy(string op) => _operator = op;
    
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var value = context.SearchValue;
        
        if (value is DateTime dt)
            return $"{field} {_operator} to_timestamp('{dt:dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS') ";
        
        return $"{field} {_operator} {value} ";
    }
    
    public override (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var paramName = $"@p{paramIndex}";
        var sql = $"{field} {_operator} {paramName} ";
        return (sql, new Dictionary<string, object> { [paramName] = context.SearchValue! });
    }
}

/// <summary>
/// Strategy for IN/NOT IN conditions.
/// </summary>
public class InStrategy : BaseConditionStrategy
{
    private readonly bool _notIn;
    
    public InStrategy(bool notIn = false) => _notIn = notIn;
    
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var op = _notIn ? "NOT IN" : "IN";
        
        if (context.SearchValue is IEnumerable<object> values)
        {
            var valueList = string.Join(",", values.Select(v => $"'{v}'"));
            return $"{field} {op} ({valueList}) ";
        }
        
        return $"{field} {op} ('{context.SearchValue}') ";
    }
    
    public override (string Sql, Dictionary<string, object> Parameters) BuildParameterized(ConditionContext context, int paramIndex)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var op = _notIn ? "NOT IN" : "IN";
        var paramName = $"@p{paramIndex}";
        
        // For PostgreSQL, use = ANY(@param) for better performance
        var sql = _notIn 
            ? $"{field} != ALL({paramName}) "
            : $"{field} = ANY({paramName}) ";
            
        return (sql, new Dictionary<string, object> { [paramName] = context.SearchValue! });
    }
}

/// <summary>
/// Strategy for NOT EQUAL condition.
/// </summary>
public class NotEqualStrategy : BaseConditionStrategy
{
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        var value = context.SearchValue;
        
        if (value == null)
            return $"{field} IS NOT NULL ";
        
        return value.GetType().FullName switch
        {
            "System.DateTime" => $"{field} != to_timestamp('{((DateTime)value):dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS') ",
            "System.String" => $"{field} != '{value}' ",
            _ when double.TryParse(value.ToString(), out _) => $"{field} != {value} ",
            _ => $"{field} != '{value}' "
        };
    }
}

/// <summary>
/// Strategy for date/time range conditions.
/// </summary>
public class DateRangeStrategy : BaseConditionStrategy
{
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        
        if (context.SearchValue is not object[] dates || dates.Length == 0)
            return string.Empty;
        
        var startDate = DateTime.Parse(dates[0].ToString()!);
        var startSql = $"to_timestamp('{startDate:dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS')";
        
        if (dates.Length == 1 || dates[1] == null)
            return $"{field} >= {startSql} ";
        
        var endDate = DateTime.Parse(dates[1].ToString()!);
        var endSql = $"to_timestamp('{endDate:dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS')";
        
        return $"({field} >= {startSql} AND {field} <= {endSql}) ";
    }
}

/// <summary>
/// Strategy for date part conditions (day, month).
/// </summary>
public class DatePartStrategy : BaseConditionStrategy
{
    private readonly string _part;
    
    public DatePartStrategy(string part) => _part = part;
    
    public override string Build(ConditionContext context)
    {
        var field = QuoteField(context.TableAlias, NormalizeFieldName(context.SearchField));
        return $"date_part('{_part}', {field}) = {context.SearchValue} ";
    }
}
