namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;

/// <summary>
/// Interface cho các DTOs có Code
/// </summary>
public interface IBaseDto
{
    string Code { get; set; }
}

// ==================== USER ====================
public class UserDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } // Ensure IsActive is exposed
    public DateTime? CreatedAt { get; set; }
    public string? Token { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; } // Optional: Admin resetting password
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

// ==================== PRODUCT ====================
public class ProductDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    [Reference("TblCategory", "CategoryCode", "Name")]
    public string? CategoryName { get; set; }
    public int? StockQuantity { get; set; }

    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? Power { get; set; }
    public string? Voltage { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public bool? IsActive { get; set; }
    public string? Brand { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? CreatedAt { get; set; }
    [ReferenceCollection(typeof(ProductImageDto), "TblFile", "MasterCode", "Code", "MasterType", "Product")]
    public List<ProductImageDto> ProductImages { get; set; } = new();
}

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }

    public decimal? Weight { get; set; }
    public string? SupplierCode { get; set; } // Added
    public string? Color { get; set; }
    public string? Power { get; set; }
    public string? Voltage { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public string? Brand { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Images { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }

    public decimal? Weight { get; set; }
    public string? SupplierCode { get; set; } // Added
    public string? Color { get; set; }
    public string? Power { get; set; }
    public string? Voltage { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public bool? IsActive { get; set; }
    public string? Brand { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsFeatured { get; set; }
    public List<string>? Images { get; set; }
}

public class ProductImageDto
{
    public string Code { get; set; } = null!;
    public string? MasterCode { get; set; }
    public string? ImageURL { get; set; }
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsPrimary { get; set; }
}

// ==================== CATEGORY ====================
public class CategoryDto : IBaseDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
    [Reference("TblCategory", "ParentCode", "Name")]
    public string? ParentName { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateCategoryDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageURL { get; set; }
    public string? ParentCode { get; set; }
    public bool? IsActive { get; set; }
}

// ==================== ORDER ====================
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

// ==================== CART ====================
public class CartDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public List<CartItemDto> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class CartItemDto
{
    public string Code { get; set; } = null!;
    public string CartCode { get; set; } = null!;
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public DateTime? AddedAt { get; set; }
}

// ==================== ADDRESS ====================
public class AddressDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool? IsDefault { get; set; }
}

public class CreateAddressDto
{
    public string UserCode { get; set; } = null!;
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateAddressDto
{
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool? IsDefault { get; set; }
}

// ==================== REVIEW ====================
public class ReviewDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public string? OrderItemCode { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool? IsApproved { get; set; }
    [Reference("TblUser", "UserCode", "Username")]
    public string? UserName { get; set; }
}

public class CreateReviewDto
{
    public string UserCode { get; set; } = null!;
    public string OrderItemCode { get; set; } = null!;
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

// ==================== COUPON ====================
public class CouponDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string? PromotionCode { get; set; }
    public int? UsageCount { get; set; }
    public bool IsValid { get; set; }
}

public class CreateCouponDto
{
    public string? PromotionCode { get; set; }
}



// ==================== PAYMENT ====================
public class PaymentDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = null!;
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
}

// ==================== DASHBOARD ====================
public class DashboardStatsDto
{
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class RecentOrderDto
{
    public string Code { get; set; } = null!;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? OrderDate { get; set; }
}

public class TopProductDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

// ==================== PAGED RESULT ====================
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalItems { get; set; }

    public PagedResult(IEnumerable<T> items, int totalItems)
    {
        Items = items;
        TotalItems = totalItems;
    }

    public static PagedResult<T> Empty() =>
        new(Enumerable.Empty<T>(), 0);
}

// ==================== SUPPLIER DTO ====================
public class SupplierDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateSupplierDto
{
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierDto
{
    public string? Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}

// ==================== QUOTE DTO ====================
public class QuoteDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    [Reference("TblUser", "UserCode", "Username")]
    public string? UserName { get; set; }
    public string ProductCode { get; set; } = null!;
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public decimal? QuotedPrice { get; set; }
    public string? AdminNote { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}

public class CreateQuoteDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Company { get; set; }
}

public class UpdateQuoteDto
{
    public int? Quantity { get; set; }
    public string? Note { get; set; }
    public string? Status { get; set; }
    public decimal? QuotedPrice { get; set; }
    public string? AdminNote { get; set; }
}
