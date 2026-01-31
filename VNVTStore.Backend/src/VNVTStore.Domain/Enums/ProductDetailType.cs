using System.ComponentModel;

namespace VNVTStore.Domain.Enums;

public enum ProductDetailType
{
    [Description("SPEC")]
    SPEC,
    
    [Description("LOGISTICS")]
    LOGISTICS,
    
    [Description("RELATION")]
    RELATION,

    [Description("IMAGE")]
    IMAGE
}
