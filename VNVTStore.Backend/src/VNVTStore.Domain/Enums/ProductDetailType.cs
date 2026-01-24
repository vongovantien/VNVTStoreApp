using System.ComponentModel;

namespace VNVTStore.Domain.Enums;

public enum ProductDetailType
{
    [Description("SPEC")]
    Spec,
    
    [Description("LOGISTICS")]
    Logistics,
    
    [Description("RELATION")]
    Relation
}
