namespace VNVTStore.Application.DTOs;

public class ReferenceTable
{
    public string TableName { get; set; } = null!;
    public string ForeignKeyCol { get; set; } = null!;
    public string ColumnName { get; set; } = null!; // Column to select (e.g., Name, Path)
    public string AliasName { get; set; } = null!; // DTO property name
    public string FilterType { get; set; } = "All";
    public string TargetColumn { get; set; } = "Code"; // Join target column
    public string? FilterColumn { get; set; }
    public string? FilterValue { get; set; }
}
