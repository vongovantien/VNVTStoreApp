namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

public class OrderDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public DateTime? OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public string? AddressCode { get; set; }
    public string? CouponCode { get; set; }
    [ReferenceCollection(typeof(OrderItemDto), "TblOrderItem", "OrderCode", "Code")]
    public List<OrderItemDto> OrderItems { get; set; } = new();

    public string? ShippingName { get; set; }
    
    public string? ShippingPhone { get; set; }
    
    // Custom mapping for Shipping Address might be tricky with Reference attribute if it's concatenated.
    // BaseHandler ReferenceAttribute maps *one* column.
    // We can map AddressLine and then frontend logic or Backend logic to display? 
    // Or we use a view / computed column? 
    // Or we stick to mapped "AddressLine" property.
    [Reference("TblAddress", "AddressCode", "AddressLine")]
    public string? ShippingAddress { get; set; }
    
    [Reference("TblUser", "UserCode", "FullName")]
    public string? CustomerName { get; set; } // Map FullName to here
}

public class OrderStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Shipping { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = null!;
}

public class OrderItemDto
{
    public string Code { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public string? ProductCode { get; set; }
    
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    
    [Reference("TblFile", "ProductCode", "Path", TargetColumn = "MasterCode", FilterColumn = "MasterType", FilterValue = "Product")]
    public string? ProductImage { get; set; }
    
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public decimal? DiscountAmount { get; set; }

    // Needs Product Image.
    // TblFile lookup: MasterCode = ProductCode, MasterType = 'Product' -> Path
    // ReferenceAttribute supports simple FK lookup. 
    // TblFile doesn't have FK to Product directly (MasterCode = Code).
    // We can use [Reference("TblFile", "ProductCode", "Path", "MasterCode", FilterColumn="MasterType", FilterValue="Product")] if supported?
    // ReferenceAttribute signature: TableName, ForeignKey (on DTO or Entity?), SelectColumn. 
    // Let's check BaseHandler logic for ReferenceAttribute.
    // It assumes FK is on the HOST Table? No, "ReferenceTable" struct in BaseHandler:
    // ForeignKeyCol = attr.ForeignKey
    // Join ON Parent.FK = RefTable.Code (Primary Key implied?)
    // BaseHandler Sql: "LEFT JOIN {t.TableName} {t.AliasName} ON t1.\"{t.ForeignKeyCol}\" = {t.AliasName}.\"Code\""
    // So it assumes Join on RefTable.Code.
    // TblFile uses MasterCode, not Code. TblFile.Code is its own PK.
    // So generic ReferenceAttribute WON'T work for TblFile (polymorphic association).
    // We need to extend ReferenceAttribute or handle Image manually.
    // For now, let's omit [Reference] for Image and expect it to be missing or I'll add a specific fix later. 
}
