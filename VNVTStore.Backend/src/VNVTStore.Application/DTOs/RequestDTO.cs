namespace VNVTStore.Application.DTOs;

/// <summary>
/// Generic Request DTO - dùng chung cho tất cả API requests
/// </summary>
public class RequestDTO
{
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public dynamic? PostObject { get; set; }
    public List<string>? Fields { get; set; }
    public SortDTO? SortDTO { get; set; }
    public List<SearchDTO>? Searching { get; set; }
}

/// <summary>
/// Generic Request DTO với typed PostObject
/// </summary>
public class RequestDTO<T>
{
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public T? PostObject { get; set; }
    public List<string>? Fields { get; set; }
    public SortDTO? SortDTO { get; set; }
    public List<SearchDTO>? Searching { get; set; }
}

/// <summary>
/// Sort DTO cho sorting
/// </summary>
/// <summary>
/// Sort DTO for sorting
/// </summary>
public class SortDTO
{
    public string? SortBy { get; set; }
    public string Sort { get; set; } = "ASC"; // "ASC" or "DESC"
    public bool SortDescending 
    { 
        get => Sort?.Equals("DESC", StringComparison.OrdinalIgnoreCase) ?? false; 
        set => Sort = value ? "DESC" : "ASC";
    }
}

/// <summary>
/// Search DTO for filtering
/// </summary>
public class SearchDTO
{
    [System.Text.Json.Serialization.JsonPropertyName("field")]
    [Newtonsoft.Json.JsonProperty("field")]
    public string SearchField { get; set; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("operator")]
    [Newtonsoft.Json.JsonProperty("operator")]
    public SearchCondition SearchCondition { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("value")]
    [Newtonsoft.Json.JsonProperty("value")]
    public object? SearchValue { get; set; }

    public short? GroupID { get; set; }
    public string? CombineCondition { get; set; } // "AND" or "OR"
}
