namespace VNVTStore.Application.Common.Attributes;

/// <summary>
/// Attribute to mark properties that should be populated with child collections
/// from a related table after the main query is executed.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ReferenceCollectionAttribute : Attribute
{
    /// <summary>
    /// Name of the child table (e.g., "TblFile")
    /// </summary>
    public string ChildTableName { get; }
    
    /// <summary>
    /// Foreign key column in the child table that references the parent (e.g., "MasterCode")
    /// </summary>
    public string ForeignKey { get; }
    
    /// <summary>
    /// Primary key column in the parent table (e.g., "Code")
    /// </summary>
    public string ParentKey { get; }
    
    /// <summary>
    /// Optional filter column for polymorphic relations (e.g., "MasterType")
    /// </summary>
    public string? FilterColumn { get; }
    
    /// <summary>
    /// Optional filter value for polymorphic relations (e.g., "Product")
    /// </summary>
    public string? FilterValue { get; }
    
    /// <summary>
    /// Type of the child DTO (e.g., typeof(ProductImageDto))
    /// </summary>
    public Type ChildDtoType { get; }

    public ReferenceCollectionAttribute(
        Type childDtoType,
        string childTableName, 
        string foreignKey, 
        string parentKey = "Code",
        string? filterColumn = null,
        string? filterValue = null)
    {
        ChildDtoType = childDtoType;
        ChildTableName = childTableName;
        ForeignKey = foreignKey;
        ParentKey = parentKey;
        FilterColumn = filterColumn;
        FilterValue = filterValue;
    }
}
