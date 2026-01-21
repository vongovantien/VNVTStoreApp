using System;

namespace VNVTStore.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ReferenceAttribute : Attribute
{
    public string TableName { get; }
    public string ForeignKey { get; }
    public string SelectColumn { get; }
    public string FilterType { get; }

    public ReferenceAttribute(string tableName, string foreignKey, string selectColumn = "Name", string filterType = "All")
    {
        TableName = tableName;
        ForeignKey = foreignKey;
        SelectColumn = selectColumn;
        FilterType = filterType;
    }
}
