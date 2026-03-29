using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Newtonsoft.Json.Linq;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Strategies;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Common.Helpers;

/// <summary>
/// Result of query building with parameterized SQL.
/// </summary>
public record QueryResult(string Sql, DynamicParameters Parameters);

/// <summary>
/// Optimized SQL Query Builder using Strategy Pattern and Parameterized Queries.
/// NEVER manually escape quotes - always use parameters for SQL injection prevention.
/// </summary>
public static class QueryBuilder
{
    /// <summary>
    /// Builds WHERE clause with parameterized conditions.
    /// Returns (SQL, Parameters) for safe Dapper execution.
    /// </summary>
    public static (string Sql, DynamicParameters Parameters) BuildRawQueryConditionParameterized(
        List<SearchDTO>? searchFieldList, 
        string? tableName = null,
        List<ReferenceTable>? refTblList = null,
        Dictionary<string, string>? aliasMap = null)
    {
        var parameters = new DynamicParameters();
        
        if (searchFieldList == null || !searchFieldList.Any())
            return (string.Empty, parameters);

        NormalizeSearchFields(searchFieldList);

        var sb = new StringBuilder();
        var paramIndex = 0;

        // Process grouped conditions
        ProcessGroupedConditionsParameterized(sb, searchFieldList, tableName, parameters, ref paramIndex, refTblList, aliasMap);
        
        // Process ungrouped conditions
        ProcessUngroupedConditionsParameterized(sb, searchFieldList, tableName, parameters, ref paramIndex, refTblList, aliasMap);

        return (sb.ToString(), parameters);
    }

    /// <summary>
    /// Builds WHERE clause (backward compatible - uses string interpolation).
    /// WARNING: Only use with trusted, validated input.
    /// </summary>
    public static string BuildRawQueryCondition(List<SearchDTO>? searchFieldList, string? tableName = null)
    {
        if (searchFieldList == null || !searchFieldList.Any())
            return string.Empty;

        NormalizeSearchFields(searchFieldList);

        var sb = new StringBuilder();
        
        ProcessGroupedConditions(sb, searchFieldList, tableName);
        ProcessUngroupedConditions(sb, searchFieldList, tableName);

        return sb.ToString();
    }

    /// <summary>
    /// Builds complete paging query with parameterized conditions.
    /// Returns QueryResult with SQL and Parameters for safe execution.
    /// </summary>
    public static QueryResult BuildRawQueryPagingParameterized(
        int pageSize, 
        int pageIndex, 
        string rootTbl, 
        List<ReferenceTable>? refTblList, 
        List<SearchDTO>? searchFieldList, 
        SortDTO? sortDTO, 
        List<string>? fields = null)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var parameters = new DynamicParameters();
        
        const string rootAlias = "r";
        sortDTO ??= new SortDTO { SortBy = "Name", Sort = "ASC" };
        if (string.IsNullOrEmpty(sortDTO.Sort)) 
            sortDTO.Sort = sortDTO.SortDescending ? "DESC" : "ASC";
        
        sortDTO.SortBy = SqlBuilderHelpers.NormalizeFieldName(sortDTO.SortBy);

        var sb = new StringBuilder();
        
        // Build SELECT clause
        // Ensure Primary Key (Code) and Sort field are included if partial fields are used
        if (fields != null && fields.Any())
        {
            var requestedFields = fields.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!requestedFields.Contains("Code") && !requestedFields.Contains(rootAlias + ".Code"))
            {
                fields.Add("Code");
            }
            if (!requestedFields.Contains(sortDTO.SortBy) && !requestedFields.Contains(rootAlias + "." + sortDTO.SortBy))
            {
                fields.Add(sortDTO.SortBy);
            }
        }

        sb.Append("WITH TempResult AS(\r\n    ");
        BuildSelectClause(sb, rootAlias, fields, refTblList);
        sb.Append(" FROM \"").Append(rootTbl).Append("\" AS ").Append(rootAlias);
        
        // Build JOINs
        // Build JOINs and get alias mapping for the WHERE clause
        var aliasMap = BuildJoinClauses(sb, rootAlias, refTblList, fields, searchFieldList);
        
        // Build WHERE clause with parameters and alias mapping for correct aliasing
        var (whereClause, whereParams) = BuildRawQueryConditionParameterized(searchFieldList, rootAlias, refTblList, aliasMap);
        parameters.AddDynamicParams(whereParams);
        
        if (!string.IsNullOrEmpty(whereClause))
        {
            sb.Append(" WHERE ").Append(whereClause);
        }
        
        sb.AppendLine();
        sb.AppendLine("), TempCount AS(");
        sb.AppendLine("    SELECT COUNT(*) AS \"TotalRow\" FROM TempResult");
        sb.AppendLine(")");
        sb.AppendLine("SELECT * FROM TempResult, TempCount");
        sb.Append("ORDER BY TempResult.\"").Append(sortDTO.SortBy).Append("\" ").AppendLine(sortDTO.Sort);
        
        // Use parameters for pagination too
        parameters.Add("@PageOffset", (pageIndex - 1) * pageSize);
        parameters.Add("@PageSize", pageSize);
        sb.AppendLine("LIMIT @PageSize OFFSET @PageOffset");

        return new QueryResult(sb.ToString(), parameters);
    }

    /// <summary>
    /// Builds complete paging query (backward compatible - string based).
    /// </summary>
    public static string BuildRawQueryPaging(
        int pageSize, 
        int pageIndex, 
        string rootTbl, 
        List<ReferenceTable>? refTblList, 
        List<SearchDTO>? searchFieldList, 
        SortDTO? sortDTO, 
        List<string>? fields = null)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        const string rootAlias = "r";
        sortDTO ??= new SortDTO { SortBy = "Code", Sort = "DESC" };
        if (string.IsNullOrEmpty(sortDTO.Sort)) 
            sortDTO.Sort = sortDTO.SortDescending ? "DESC" : "ASC";
        
        sortDTO.SortBy = SqlBuilderHelpers.NormalizeFieldName(sortDTO.SortBy);

        var sb = new StringBuilder();
        
        sb.Append("WITH TempResult AS(\r\n    ");
        BuildSelectClause(sb, rootAlias, fields, refTblList);
        sb.Append(" FROM \"").Append(rootTbl).Append("\" AS ").Append(rootAlias);
        
        BuildJoinClauses(sb, rootAlias, refTblList, fields, searchFieldList);
        
        var whereClause = BuildRawQueryCondition(searchFieldList, rootAlias);
        if (!string.IsNullOrEmpty(whereClause))
        {
            sb.Append(" WHERE ").Append(whereClause);
        }
        
        sb.AppendLine();
        sb.AppendLine("), TempCount AS(");
        sb.AppendLine("    SELECT COUNT(*) AS \"TotalRow\" FROM TempResult");
        sb.AppendLine(")");
        sb.AppendLine("SELECT * FROM TempResult, TempCount");
        sb.Append("ORDER BY TempResult.\"").Append(sortDTO.SortBy).Append("\" ").AppendLine(sortDTO.Sort);
        sb.Append("LIMIT ").Append(pageSize).Append(" OFFSET (").Append(pageIndex).Append(" - 1) * ").AppendLine(pageSize.ToString());

        var finalSql = sb.ToString();
        return finalSql;
    }

    #region Parameterized Query Builders

    private static void ProcessGroupedConditionsParameterized(
        StringBuilder sb, 
        List<SearchDTO> searchFields, 
        string? tableName,
        DynamicParameters parameters,
        ref int paramIndex,
        List<ReferenceTable>? refTblList = null,
        Dictionary<string, string>? aliasMap = null)
    {
        var groupedItems = searchFields.Where(x => x.GroupID.HasValue).ToList();
        if (!groupedItems.Any()) return;

        var groups = groupedItems.GroupBy(x => x.GroupID).Select(g => g.Key).ToList();
        var groupIdx = 0;

        foreach (var groupId in groups)
        {
            var groupSb = new StringBuilder();
            var itemsInGroup = groupedItems.Where(x => x.GroupID == groupId).ToList();

            foreach (var item in itemsInGroup)
            {
                if (string.IsNullOrEmpty(item.SearchField)) continue;

                if (groupSb.Length > 0)
                {
                    groupSb.Append(string.IsNullOrEmpty(item.CombineCondition) ? " AND " : $" {item.CombineCondition} ");
                }

                var condition = BuildConditionParameterized(item, tableName, parameters, ref paramIndex, refTblList, aliasMap);
                groupSb.Append(condition);
            }

            if (groupSb.Length > 0)
            {
                if (groupIdx > 0) sb.Append(" OR ");
                sb.Append("( ").Append(groupSb).Append(" )");
                groupIdx++;
            }
        }
    }

    private static void ProcessUngroupedConditionsParameterized(
        StringBuilder sb, 
        List<SearchDTO> searchFields, 
        string? tableName,
        DynamicParameters parameters,
        ref int paramIndex,
        List<ReferenceTable>? refTblList = null,
        Dictionary<string, string>? aliasMap = null)
    {
        var ungroupedItems = searchFields.Where(x => !x.GroupID.HasValue).ToList();

        foreach (var item in ungroupedItems)
        {
            if (string.IsNullOrEmpty(item.SearchField)) continue;

            if (sb.Length > 0)
            {
                sb.Append(string.IsNullOrEmpty(item.CombineCondition) ? " AND " : $" {item.CombineCondition} ");
            }

            var condition = BuildConditionParameterized(item, tableName, parameters, ref paramIndex, refTblList, aliasMap);
            sb.Append(condition);
        }
    }

    private static string BuildConditionParameterized(
        SearchDTO item, 
        string? tableName, 
        DynamicParameters parameters,
        ref int paramIndex,
        List<ReferenceTable>? refTblList = null,
        Dictionary<string, string>? aliasMap = null)
    {
        try
        {
            var searchField = item.SearchField;
            var currentTableName = tableName;

            // Handle joined reference fields (e.g. "Brand", "CategoryName")
            if (aliasMap != null && aliasMap.TryGetValue(searchField, out var mappedAlias))
            {
                currentTableName = mappedAlias;
                // If the mapped field in the join is different from AliasName, we should use refTbl.ColumnName
                // But aliasMap currently only stores TargetAlias. We need to know the ColumnName too.
                if (refTblList != null)
                {
                    var refTbl = refTblList.FirstOrDefault(r => string.Equals(r.AliasName, searchField, StringComparison.OrdinalIgnoreCase));
                    if (refTbl != null) searchField = refTbl.ColumnName;
                }
            }
            else if (searchField.Contains('.'))
            {
                // Explicit alias provided in SearchField (e.g. "t1.Name")
                var parts = searchField.Split('.');
                currentTableName = parts[0];
                searchField = parts[1];
            }

            var field = SqlBuilderHelpers.QuoteField(currentTableName, searchField);

            // Unwrap JsonElement if present
            item.SearchValue = UnwrapJsonElement(item.SearchValue);

            // FIX: Convert numeric strings to actual numbers to avoid PostgreSQL type mismatch (numeric vs text)
            if (IsNumericField(item.SearchField) && item.SearchValue is string strNum && decimal.TryParse(strNum, out var decimalValue))
            {
                 item.SearchValue = decimalValue;
            }

            // FIX: Convert date strings to DateTime for typed parameters (avoid operator does not exist: timestamp >= text)
            if (IsDateField(item.SearchField) && item.SearchValue is string strDate && DateTime.TryParse(strDate, out var dateValue))
            {
                 item.SearchValue = dateValue;
            }

            // Handle array values
            if (JsonUtilities.CheckJsonArray(item.SearchValue) || 
                (item.SearchValue is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Array) ||
                (item.SearchValue != null && item.SearchValue.GetType().IsArray) ||
                (item.SearchValue is System.Collections.IList && item.SearchValue.GetType() != typeof(string)))
            {
                // Ensure SearchValue is properly converted to a list/array if it's a JsonElement Array
                if (item.SearchValue is System.Text.Json.JsonElement jeArray)
                {
                     // Convert generic JsonElement Array to object array
                     var list = new List<object>();
                     foreach (var element in jeArray.EnumerateArray())
                     {
                         list.Add(UnwrapJsonElement(element));
                     }
                     item.SearchValue = list.ToArray();
                }

                return BuildArrayConditionParameterized(item, field, parameters, ref paramIndex);
            }

            // Use parameterized approach based on condition type
            return item.SearchCondition switch
            {
                SearchCondition.IsNull => $"{field} IS NULL ",
                SearchCondition.IsNotNull => $"{field} IS NOT NULL ",
                SearchCondition.Contains => BuildContainsParam(field, item.SearchValue, parameters, ref paramIndex),
                SearchCondition.Equal => BuildEqualParam(field, item.SearchValue, parameters, ref paramIndex),
                SearchCondition.EqualExact => BuildEqualExactParam(field, item.SearchValue, parameters, ref paramIndex),
                SearchCondition.NotEqual => BuildNotEqualParam(field, item.SearchValue, parameters, ref paramIndex),
                SearchCondition.GreaterThan => BuildComparisonParam(field, ">", item.SearchValue, parameters, ref paramIndex),
                SearchCondition.GreaterThanEqual => BuildComparisonParam(field, ">=", item.SearchValue, parameters, ref paramIndex),
                SearchCondition.LessThan => BuildComparisonParam(field, "<", item.SearchValue, parameters, ref paramIndex),
                SearchCondition.LessThanEqual => BuildComparisonParam(field, "<=", item.SearchValue, parameters, ref paramIndex),
                SearchCondition.In => BuildInParam(field, new[] { item.SearchValue }, false, parameters, ref paramIndex),
                SearchCondition.NotIn => BuildInParam(field, new[] { item.SearchValue }, true, parameters, ref paramIndex),
                _ => string.Empty
            };
        }
        catch (Exception ex)
        {
            // Error building condition - return empty to skip
            return string.Empty;
        }
    }

    private static object? UnwrapJsonElement(object? value)
    {
        if (value is System.Text.Json.JsonElement element)
        {
            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.String:
                    return element.GetString();
                case System.Text.Json.JsonValueKind.Number:
                    if (element.TryGetInt32(out var i)) return i;
                    if (element.TryGetInt64(out var l)) return l;
                    if (element.TryGetDouble(out var d)) return d;
                    return element.GetRawText();
                case System.Text.Json.JsonValueKind.True:
                    return true;
                case System.Text.Json.JsonValueKind.False:
                    return false;
                case System.Text.Json.JsonValueKind.Null:
                    return null;
                // Arrays are handled by caller or recursion if needed, but here likely scalar unless array in array
                case System.Text.Json.JsonValueKind.Array:
                     var list = new List<object>();
                     foreach (var e in element.EnumerateArray())
                     {
                         list.Add(UnwrapJsonElement(e));
                     }
                     return list.ToArray();
                default:
                    return element.GetRawText();
            }
        }
        return value;
    }

    private static bool IsBooleanField(string field)
    {
        if (string.IsNullOrEmpty(field)) return false;
        
        // Remove quotes if present
        var fieldName = field.Replace("\"", "").Trim();
        // Remove table alias if present (e.g., "r.IsActive" -> "IsActive")
        if (fieldName.Contains("."))
        {
            fieldName = fieldName.Split('.').Last();
        }

        var booleanFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "IsActive", "IsApproved", "IsNew", "IsFeatured", "IsDefault", 
            "IsEmailVerified", "IsPublic", "IsLocked", "IsValid", "IsDeleted"
        };
        
        return booleanFields.Contains(fieldName);
    }

    private static string BuildContainsParam(string field, object? value, DynamicParameters parameters, ref int paramIndex)
    {
        if (value == null) return string.Empty;
        
        if (IsBooleanField(field))
        {
            return BuildEqualParam(field, value, parameters, ref paramIndex);
        }

        var paramName = $"p{paramIndex++}";
        parameters.Add(paramName, $"%{value}%");
        return $"{field}::text ILIKE @{paramName} ";
    }

    private static string BuildEqualParam(string field, object? value, DynamicParameters parameters, ref int paramIndex)
    {
        if (value == null) return $"{field} IS NULL ";
        var paramName = $"p{paramIndex++}";
        
        if (value is DateTime dt)
        {
            parameters.Add(paramName, dt);
            return $"{field} = @{paramName} ";
        }
        
        if (value is string || value is System.Text.Json.JsonElement)
        {
            var stringValue = value.ToString()?.Trim();
            
            if (IsBooleanField(field))
            {
                bool boolValue = false;
                if (string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase) || stringValue == "1")
                    boolValue = true;
                else if (string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase) || stringValue == "0")
                    boolValue = false;
                else
                    return " (1=2) "; // Invalid boolean search value, should not match anything

                parameters.Add(paramName, boolValue);
                return $"{field} = @{paramName} ";
            }

            parameters.Add(paramName, stringValue);
            return $"{field}::text ILIKE @{paramName} ";
        }
        
        parameters.Add(paramName, value);
        return $"{field} = @{paramName} ";
    }

    private static string BuildEqualExactParam(string field, object? value, DynamicParameters parameters, ref int paramIndex)
    {
        if (value == null) return $"{field} IS NULL ";
        var paramName = $"p{paramIndex++}";
        parameters.Add(paramName, value);
        return $"{field} = @{paramName} ";
    }

    private static string BuildNotEqualParam(string field, object? value, DynamicParameters parameters, ref int paramIndex)
    {
        if (value == null) return $"{field} IS NOT NULL ";
        var paramName = $"p{paramIndex++}";
        parameters.Add(paramName, value);
        // Fix: NULL != 'Value' is UNKNOWN in SQL, so it filters out NULLs.
        // For search filters, we usually want NULLs included if they don't match the excluded value.
        return $"({field} IS NULL OR {field} != @{paramName}) ";
    }

    private static string BuildComparisonParam(string field, string op, object? value, DynamicParameters parameters, ref int paramIndex)
    {
        if (value == null) return string.Empty;
        var paramName = $"p{paramIndex++}";
        parameters.Add(paramName, value);
        return $"{field} {op} @{paramName} ";
    }

    private static string BuildInParam(string field, object[] values, bool notIn, DynamicParameters parameters, ref int paramIndex)
    {
        var paramName = $"p{paramIndex++}";
        parameters.Add(paramName, values);
        // PostgreSQL: Use = ANY(@param) for IN, != ALL(@param) for NOT IN
        return notIn 
            ? $"{field} != ALL(@{paramName}) "
            : $"{field} = ANY(@{paramName}) ";
    }

    private static string BuildArrayConditionParameterized(
        SearchDTO item, 
        string field, 
        DynamicParameters parameters, 
        ref int paramIndex)
    {
        object[] values;
        
        if (item.SearchValue is JArray jArray)
        {
            values = jArray.ToObject<object[]>()!;
        }
        else if (item.SearchValue is object[] objArray)
        {
            values = objArray;
        }
        else
        {
             // Fallback or empty
             values = Array.Empty<object>();
        }

        return item.SearchCondition switch
        {
            SearchCondition.In => BuildInParam(field, values, false, parameters, ref paramIndex),
            SearchCondition.NotIn => BuildInParam(field, values, true, parameters, ref paramIndex),
            SearchCondition.DateTimeRange => BuildDateRangeParam(field, values, parameters, ref paramIndex),
            SearchCondition.Contains => BuildContainsArrayParam(field, values, parameters, ref paramIndex),
            SearchCondition.Equal => BuildEqualArrayParam(field, values, parameters, ref paramIndex),
            _ => string.Empty
        };
    }

    private static string BuildDateRangeParam(string field, object[] values, DynamicParameters parameters, ref int paramIndex)
    {
        if (values.Length == 0) return string.Empty;
        
        var startDate = DateTime.Parse(values[0].ToString()!);
        var startParam = $"p{paramIndex++}";
        parameters.Add(startParam, startDate);

        if (values.Length == 1 || values[1] == null)
            return $"{field} >= @{startParam} ";

        var endDate = DateTime.Parse(values[1].ToString()!);
        var endParam = $"p{paramIndex++}";
        parameters.Add(endParam, endDate);

        return $"({field} >= @{startParam} AND {field} <= @{endParam}) ";
    }

    private static string BuildContainsArrayParam(string field, object[] values, DynamicParameters parameters, ref int paramIndex)
    {
        var conditions = new List<string>();
        foreach (var v in values)
        {
            var paramName = $"p{paramIndex++}";
            parameters.Add(paramName, $"%{v}%");
            conditions.Add($"{field}::text ILIKE @{paramName}");
        }
        return $"({string.Join(" OR ", conditions)}) ";
    }

    private static string BuildEqualArrayParam(string field, object[] values, DynamicParameters parameters, ref int paramIndex)
    {
        var conditions = new List<string>();
        foreach (var v in values)
        {
            var paramName = $"p{paramIndex++}";
            parameters.Add(paramName, v);
            conditions.Add(v is string ? $"{field}::text ILIKE @{paramName}" : $"{field} = @{paramName}");
        }
        return $"({string.Join(" OR ", conditions)}) ";
    }

    #endregion

    #region Non-Parameterized Helpers (Backward Compatibility)

    private static void NormalizeSearchFields(List<SearchDTO> searchFields)
    {
        foreach (var item in searchFields)
        {
            if (!string.IsNullOrEmpty(item.SearchField) && char.IsLower(item.SearchField[0]))
            {
                item.SearchField = char.ToUpper(item.SearchField[0]) + item.SearchField.Substring(1);
            }
        }
    }

    private static void ProcessGroupedConditions(StringBuilder sb, List<SearchDTO> searchFields, string? tableName)
    {
        var groupedItems = searchFields.Where(x => x.GroupID.HasValue).ToList();
        if (!groupedItems.Any()) return;

        var groups = groupedItems.GroupBy(x => x.GroupID).Select(g => g.Key).ToList();
        var groupIndex = 0;

        foreach (var groupId in groups)
        {
            var groupSb = new StringBuilder();
            var itemsInGroup = groupedItems.Where(x => x.GroupID == groupId).ToList();

            foreach (var item in itemsInGroup)
            {
                if (string.IsNullOrEmpty(item.SearchField)) continue;

                if (groupSb.Length > 0)
                {
                    groupSb.Append(string.IsNullOrEmpty(item.CombineCondition) ? " AND " : $" {item.CombineCondition} ");
                }

                var condition = BuildCondition(item, tableName);
                groupSb.Append(condition);
            }

            if (groupSb.Length > 0)
            {
                if (groupIndex > 0) sb.Append(" OR ");
                sb.Append("( ").Append(groupSb).Append(" )");
                groupIndex++;
            }
        }
    }

    private static void ProcessUngroupedConditions(StringBuilder sb, List<SearchDTO> searchFields, string? tableName)
    {
        var ungroupedItems = searchFields.Where(x => !x.GroupID.HasValue).ToList();

        foreach (var item in ungroupedItems)
        {
            if (string.IsNullOrEmpty(item.SearchField)) continue;

            if (sb.Length > 0)
            {
                sb.Append(string.IsNullOrEmpty(item.CombineCondition) ? " AND " : $" {item.CombineCondition} ");
            }

            var condition = BuildCondition(item, tableName);
            sb.Append(condition);
        }
    }

    private static string BuildCondition(SearchDTO item, string? tableName)
    {
        try
        {
            if (JsonUtilities.CheckJsonArray(item.SearchValue))
            {
                return BuildArrayCondition(item, tableName);
            }

            if (ConditionBuilderFactory.HasStrategy(item.SearchCondition))
            {
                var strategy = ConditionBuilderFactory.GetStrategy(item.SearchCondition);
                var context = new ConditionContext(item.SearchField, item.SearchValue, tableName);
                return strategy.Build(context);
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string BuildArrayCondition(SearchDTO item, string? tableName)
    {
        if (item.SearchValue == null) return string.Empty;
        var values = ((JArray)item.SearchValue).ToObject<object[]>()!;
        var field = SqlBuilderHelpers.QuoteField(tableName, item.SearchField);

        return item.SearchCondition switch
        {
            SearchCondition.In => BuildInClause(field, values, notIn: false),
            SearchCondition.NotIn => BuildInClause(field, values, notIn: true),
            SearchCondition.DateTimeRange => BuildDateRangeClause(field, values),
            SearchCondition.Contains => BuildContainsArrayClause(field, values),
            SearchCondition.Equal => BuildEqualArrayClause(field, values),
            SearchCondition.NotEqual => BuildNotEqualArrayClause(field, values),
            _ => string.Empty
        };
    }

    private static string BuildInClause(string field, object[] values, bool notIn)
    {
        var valueList = string.Join(",", values.Select(v => $"'{v}'"));
        var op = notIn ? "NOT IN" : "IN";
        return $"{field} {op} ({valueList}) ";
    }

    private static string BuildDateRangeClause(string field, object[] values)
    {
        if (values.Length == 0) return string.Empty;
        
        var startDate = DateTime.Parse(values[0].ToString()!);
        var startSql = $"to_timestamp('{startDate:dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS')";

        if (values.Length == 1 || values[1] == null)
            return $"{field} >= {startSql} ";

        var endDate = DateTime.Parse(values[1].ToString()!);
        var endSql = $"to_timestamp('{endDate:dd/MM/yyyy HH:mm:ss}', 'dd/MM/yyyy HH24:MI:SS')";

        return $"({field} >= {startSql} AND {field} <= {endSql}) ";
    }

    private static string BuildContainsArrayClause(string field, object[] values)
    {
        var conditions = values.Select(v => $"{field}::text ILIKE '%{v}%'");
        return $"({string.Join(" OR ", conditions)}) ";
    }

    private static string BuildEqualArrayClause(string field, object[] values)
    {
        var conditions = values.Select(v => 
            v.GetType().FullName == "System.String" 
                ? $"{field} ILIKE '{v}'" 
                : $"{field} = {v}");
        return $"({string.Join(" OR ", conditions)}) ";
    }

    private static string BuildNotEqualArrayClause(string field, object[] values)
    {
        var conditions = values.Select(v => $"{field} != '{v}'");
        return $"({string.Join(" OR ", conditions)}) ";
    }

    public static void BuildSelectClause(StringBuilder sb, string rootAlias, List<string>? fields, List<ReferenceTable>? refTblList)
    {
        sb.Append("SELECT ");
        
        if (fields == null || !fields.Any())
        {
            sb.Append(rootAlias).Append(".*");
        }
        else
        {
            var normalizedFields = fields
                .Where(f => !string.IsNullOrEmpty(f) && !f.Contains("."))
                .Select(SqlBuilderHelpers.NormalizeFieldName)
                .ToList();

            var fieldList = normalizedFields
                .Where(f => refTblList?.Any(r => r.AliasName == f) != true)
                .Select(f => $"{rootAlias}.\"{f}\"");
            
            sb.Append(string.Join(", ", fieldList));
        }
    }

    /// <summary>
    /// Builds SELECT clause for a specific alias and fields list (Used for Child Collection Partial Select)
    /// </summary>
    public static void BuildSelectClause(StringBuilder sb, string alias, List<string>? fields)
    {
        sb.Append("SELECT ");

        if (fields == null || !fields.Any())
        {
            sb.Append(alias).Append(".*");
        }
        else
        {
            var fieldList = fields
                .Where(f => !string.IsNullOrEmpty(f))
                .Select(f => $"{alias}.\"{SqlBuilderHelpers.NormalizeFieldName(f)}\"");
            
            sb.Append(string.Join(", ", fieldList));
        }
        
    }

    public static Dictionary<string, string> BuildJoinClauses(StringBuilder sb, string rootAlias, List<ReferenceTable>? refTblList, List<string>? fields, List<SearchDTO>? searchFields = null)
    {
        var aliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (refTblList == null || !refTblList.Any()) return aliasMap;

        var normalizedFields = fields?
            .Select(SqlBuilderHelpers.NormalizeFieldName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Also consider fields used in search
        HashSet<string>? searchAliases = null;
        if (searchFields != null)
        {
            foreach (var s in searchFields)
            {
                if (string.IsNullOrEmpty(s.SearchField)) continue;
                searchAliases ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (s.SearchField.Contains('.'))
                {
                    searchAliases.Add(SqlBuilderHelpers.NormalizeFieldName(s.SearchField.Split('.')[0]));
                }
                else
                {
                    // FEATURE: Detect alias usage in search fields even without dot notation
                    if (refTblList.Any(r => string.Equals(r.AliasName, s.SearchField, StringComparison.OrdinalIgnoreCase)))
                    {
                        searchAliases.Add(SqlBuilderHelpers.NormalizeFieldName(s.SearchField));
                    }
                }
            }
        }

        var tableIndex = 1;
        foreach (var refTbl in refTblList)
        {
            if (string.IsNullOrEmpty(refTbl.TableName) ||
                string.IsNullOrEmpty(refTbl.ForeignKeyCol) ||
                string.IsNullOrEmpty(refTbl.ColumnName) ||
                string.IsNullOrEmpty(refTbl.AliasName))
            {
                continue;
            }

            // Join if requested in fields OR used in search
            // If fields is null, we join everything (legacy behavior)
            if (normalizedFields != null && 
                !normalizedFields.Contains(refTbl.AliasName) && 
                (searchAliases == null || !searchAliases.Contains(refTbl.AliasName)))
            {
                continue;
            }

            var targetAlias = $"t{tableIndex}";
            aliasMap[refTbl.AliasName] = targetAlias;
            
            // Only add to SELECT if it was in normalizedFields (or normalizedFields is null)
            if (normalizedFields == null || normalizedFields.Contains(refTbl.AliasName))
            {
                sb.Insert(sb.ToString().IndexOf(" FROM"), $", {targetAlias}.\"{refTbl.ColumnName}\" AS \"{refTbl.AliasName}\"");
            }

            sb.Append(SqlBuilderHelpers.BuildLeftJoin(
                refTbl.TableName,
                targetAlias,
                rootAlias,
                refTbl.ForeignKeyCol,
                refTbl.TargetColumn ?? "Code",
                refTbl.FilterColumn,
                refTbl.FilterValue));

            tableIndex++;
        }

        return aliasMap;
    }

    private static bool IsNumericField(string field)
    {
        if (string.IsNullOrEmpty(field)) return false;
        // Remove quotes if present
        var f = field.Replace("\"", "").Trim().ToLowerInvariant();
        // Remove table alias if present
        if (f.Contains("."))
        {
            f = f.Split('.').Last();
        }

        return f.EndsWith("price") || 
               f.EndsWith("amount") || 
               f.EndsWith("quantity") || 
               f.EndsWith("fee") || 
               f.EndsWith("rate") ||
               f.EndsWith("rating") ||
               f.EndsWith("total") ||
               f.EndsWith("discount") ||
               f.EndsWith("subtotal") ||
               f == "weight" ||
               f == "stock" ||
               f == "usagecount";
    }

    private static bool IsDateField(string field)
    {
        if (string.IsNullOrEmpty(field)) return false;
        
        // Remove quotes if present
        var f = field.Replace("\"", "").Trim().ToLowerInvariant();
        // Remove table alias if present
        if (f.Contains("."))
        {
            f = f.Split('.').Last();
        }

        return f.EndsWith("at") || 
               f.EndsWith("date") || 
               f.EndsWith("time") ||
               f == "dob" ||
               f == "start" ||
               f == "end";
    }

    #endregion
}
