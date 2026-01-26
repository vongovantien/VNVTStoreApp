using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace VNVTStore.Application.Common.Helpers;

/// <summary>
/// SQL building utilities with caching (Flyweight Pattern).
/// Thread-safe and optimized for performance.
/// </summary>
public static class SqlBuilderHelpers
{
    // Flyweight: Cache quoted field names
    private static readonly ConcurrentDictionary<string, string> _quotedFieldCache = new();
    
    /// <summary>
    /// Quotes an identifier (table/column name) for PostgreSQL.
    /// </summary>
    public static string QuoteIdentifier(string name) => $"\"{name}\"";
    
    /// <summary>
    /// Quotes a field with optional table alias. Cached for performance.
    /// </summary>
    public static string QuoteField(string? tableAlias, string field)
    {
        var cacheKey = $"{tableAlias ?? ""}.{field}";
        return _quotedFieldCache.GetOrAdd(cacheKey, _ =>
            string.IsNullOrEmpty(tableAlias)
                ? $"\"{field}\""
                : $"\"{tableAlias}\".\"{field}\"");
    }
    
    /// <summary>
    /// Normalizes field name (capitalize first letter to match DB convention).
    /// </summary>
    public static string NormalizeFieldName(string field)
    {
        if (string.IsNullOrEmpty(field)) return field;
        return char.IsLower(field[0]) 
            ? char.ToUpper(field[0]) + field.Substring(1) 
            : field;
    }
    
    /// <summary>
    /// Builds SELECT clause with specific columns (avoid SELECT *).
    /// </summary>
    public static string BuildSelectClause(string tableAlias, IEnumerable<string>? fields)
    {
        if (fields == null || !fields.Any())
            return $"{tableAlias}.*";
        
        var sb = new StringBuilder();
        foreach (var field in fields)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(tableAlias).Append(".\"").Append(NormalizeFieldName(field)).Append('"');
        }
        return sb.ToString();
    }
    
    /// <summary>
    /// Builds LEFT JOIN clause.
    /// </summary>
    public static string BuildLeftJoin(
        string targetTable, 
        string targetAlias,
        string sourceAlias,
        string foreignKey,
        string targetColumn = "Code",
        string? filterColumn = null,
        string? filterValue = null)
    {
        var sb = new StringBuilder();
        sb.Append(" LEFT JOIN \"").Append(targetTable).Append("\" AS ").Append(targetAlias);
        sb.Append(" ON ").Append(sourceAlias).Append(".\"").Append(foreignKey).Append("\" = ");
        sb.Append(targetAlias).Append(".\"").Append(targetColumn).Append('"');
        
        if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
        {
            sb.Append(" AND ").Append(targetAlias).Append(".\"").Append(filterColumn).Append("\" = '")
              .Append(filterValue).Append('\'');
        }
        

        
        return sb.ToString();
    }
    
    /// <summary>
    /// Builds ORDER BY clause.
    /// </summary>
    public static string BuildOrderByClause(string sortBy, string sortDirection = "DESC")
    {
        var normalizedField = NormalizeFieldName(sortBy);
        return $"\"{normalizedField}\" {sortDirection.ToUpper()}";
    }
    
    /// <summary>
    /// Builds pagination clause (PostgreSQL OFFSET/FETCH syntax).
    /// </summary>
    public static string BuildPaginationClause(int pageSize, int pageIndex)
    {
        var offset = (pageIndex - 1) * pageSize;
        return $"OFFSET {offset} FETCH NEXT {pageSize} ROWS ONLY";
    }
    
    /// <summary>
    /// Appends quoted field to StringBuilder.
    /// </summary>
    public static StringBuilder AppendQuotedField(this StringBuilder sb, string? tableAlias, string field)
    {
        if (!string.IsNullOrEmpty(tableAlias))
        {
            sb.Append('"').Append(tableAlias).Append("\".\"");
        }
        else
        {
            sb.Append('"');
        }
        return sb.Append(NormalizeFieldName(field)).Append('"');
    }
}
