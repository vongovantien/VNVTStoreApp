namespace VNVTStore.Application.DTOs;

public class ReferenceTable
{
    public string TableName { get; set; } = null!;
    public string AliasName { get; set; } = null!;
    public string ForeignKeyCol { get; set; } = null!;
    public string ColumnName { get; set; } = null!;
    public string? FilterType { get; set; }
    public string[]? OptionJoin { get; set; }
}
