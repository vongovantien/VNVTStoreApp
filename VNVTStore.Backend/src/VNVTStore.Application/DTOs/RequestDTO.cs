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
public class SortDTO
{
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Search DTO cho filtering
/// </summary>
public class SearchDTO
{
    public string? Field { get; set; }
    public SearchCondition Operator { get; set; } // Uses new Enum
    public object? Value { get; set; }
}
