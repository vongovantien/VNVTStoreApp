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
    [System.Text.Json.Serialization.JsonPropertyName("sortBy")]
    [Newtonsoft.Json.JsonProperty("sortBy")]
    public string? SortBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("sort")]
    [Newtonsoft.Json.JsonProperty("sort")]
    public string Sort { get; set; } = "ASC"; // "ASC" or "DESC"

    /// <summary>
    /// Backward compatible computed property. Prefer using Sort directly.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonPropertyName("searchField")]
    [Newtonsoft.Json.JsonProperty("searchField")]
    public string SearchField { get; set; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("searchCondition")]
    [Newtonsoft.Json.JsonProperty("searchCondition")]
    public SearchCondition SearchCondition { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("searchValue")]
    [Newtonsoft.Json.JsonProperty("searchValue")]
    public object? SearchValue { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("groupID")]
    [Newtonsoft.Json.JsonProperty("groupID")]
    public short? GroupID { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("combineCondition")]
    [Newtonsoft.Json.JsonProperty("combineCondition")]
    public string? CombineCondition { get; set; } // "AND" or "OR"
}
