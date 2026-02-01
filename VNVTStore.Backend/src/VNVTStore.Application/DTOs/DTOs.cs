namespace VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Attributes;
using VNVTStore.Domain.Enums;

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
    public string? RoleCode { get; set; }
    [Reference("TblRole", "RoleCode", "Name")]
    public string? RoleName { get; set; }
    public bool IsActive { get; set; } // Ensure IsActive is exposed
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? Token { get; set; }
    public string? Avatar { get; set; }
    public List<string> Permissions { get; set; } = new();
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
    public string? AvatarUrl { get; set; }
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
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    [Reference("TblCategory", "CategoryCode", "Name")]
    public string? CategoryName { get; set; }
    public int? StockQuantity { get; set; }

    public string? BrandCode { get; set; }
    [Reference("TblBrand", "BrandCode", "Name")]
    public string? Brand { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    [Reference("TblSupplier", "SupplierCode", "Name")]
    public string? SupplierName { get; set; }

    public bool? IsActive { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? CreatedAt { get; set; }

    [ReferenceCollection(typeof(ProductDetailDto), "TblProductDetail", "ProductCode", "Code")]
    public List<ProductDetailDto> Details { get; set; } = new();

    [ReferenceCollection(typeof(ProductImageDto), "TblFile", "MasterCode", "Code", "MasterType", "Product")]
    public List<ProductImageDto> ProductImages { get; set; } = new();

    [ReferenceCollection(typeof(UnitDto), "TblProductUnit", "ProductCode", "Code")]
    public List<UnitDto> ProductUnits { get; set; } = new();

    [ReferenceCollection(typeof(ProductVariantDto), "TblProductVariant", "ProductCode", "Code")]
    public List<ProductVariantDto> Variants { get; set; } = new();

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class ProductDetailDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public ProductDetailType DetailType { get; set; } = ProductDetailType.SPEC;
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
}

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }
    public string? BrandCode { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public List<CreateProductDetailDto>? Details { get; set; }
    public List<CreateUnitDto>? ProductUnits { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
}

public class CreateProductDetailDto
{
    public ProductDetailType DetailType { get; set; } = ProductDetailType.SPEC;
    public string SpecName { get; set; } = null!;
    public string SpecValue { get; set; } = null!;
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? CategoryCode { get; set; }
    public int? StockQuantity { get; set; }
    public string? BrandCode { get; set; }
    public string? BaseUnit { get; set; }
    public int? MinStockLevel { get; set; }
    public string? BinLocation { get; set; }
    public decimal? VatRate { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SupplierCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsFeatured { get; set; }
    public List<string>? Images { get; set; }
    public List<CreateProductDetailDto>? Details { get; set; } // reuse Create dto for list replacement
    public List<CreateUnitDto>? ProductUnits { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
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

public class ProductStatsDto
{
    public int Total { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}

public class ProductVariantDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public string SKU { get; set; } = null!;
    public string? Attributes { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductVariantDto
{
    public string SKU { get; set; } = null!;
    public string? Attributes { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class UpdateProductVariantDto
{
    public string? SKU { get; set; }
    public string? Attributes { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool? IsActive { get; set; }
}

// ==================== BRAND ====================
public class BrandDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBrandDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}

public class UpdateBrandDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
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

public class CategoryStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Main { get; set; }
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

public class OrderStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Shipping { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
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
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool? IsDefault { get; set; }
}

public class CreateAddressDto
{
    public string? UserCode { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateAddressDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
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
    public string? AdminReply { get; set; }
    [Reference("TblUser", "UserCode", "Username")]
    public string? UserName { get; set; }
    
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    
    public string? ProductCode { get; set; }
}

public class CreateReviewDto
{
    public string UserCode { get; set; } = null!;
    public string? OrderItemCode { get; set; }
    public string? ProductCode { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool? IsApproved { get; set; }
    public string? AdminReply { get; set; }
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
    
    public decimal TotalAmount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    public string? Note { get; set; }
    public string? AdminNote { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    [ReferenceCollection(typeof(QuoteItemDto), "TblQuoteItem", "QuoteCode", "Code")]
    public List<QuoteItemDto> Items { get; set; } = new();
}

public class QuoteItemDto
{
    public string Code { get; set; } = null!;
    public string QuoteCode { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    [Reference("TblProduct", "ProductCode", "Name")]
    public string? ProductName { get; set; }
    
    public string? UnitCode { get; set; }
    [Reference("TblUnit", "UnitCode", "Name")]
    public string? UnitName { get; set; }
    
    public int Quantity { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal ApprovedPrice { get; set; }
    public decimal TotalLineAmount { get; set; }
}

public class CreateQuoteDto
{
    public string? Note { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Company { get; set; }
    public List<CreateQuoteItemDto> Items { get; set; } = new();
}

public class CreateQuoteItemDto
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string? UnitCode { get; set; }
    public decimal? RequestPrice { get; set; }
}

public class UpdateQuoteDto
{
    public string? Note { get; set; }
    public string? Status { get; set; }
    public string? AdminNote { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
    // For modifying items, typically we use separate endpoints or full replace
    public List<UpdateQuoteItemDto>? Items { get; set; } 
}

public class UpdateQuoteItemDto 
{
    public string? Code { get; set; } // If null, new item
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal? ApprovedPrice { get; set; }
}

// ==================== UNIT DTO ====================
public class UnitDto : IBaseDto
{
    public string Code { get; set; } = null!; // ProductUnit Code
    public string ProductCode { get; set; } = null!;
    public string? UnitCode { get; set; } // MasterUnit Code
    public string UnitName { get; set; } = null!; // MasterUnit Name
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUnitDto
{
    public string? ProductCode { get; set; } 
    public string UnitName { get; set; } = null!;
    public decimal ConversionRate { get; set; }
    public decimal Price { get; set; }
    public bool IsBaseUnit { get; set; }
}

public class UpdateUnitDto
{
    public string? UnitName { get; set; }
    public decimal? ConversionRate { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
}

// ==================== TAG DTO ====================
public class TagDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = null!;
}

public class UpdateTagDto
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public class EntityStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
}

// ==================== NEWS DTO ====================
public class NewsDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Thumbnail { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsActive { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateNewsDto
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Thumbnail { get; set; }
    public string? Author { get; set; }
    public bool IsActive { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; }
}

public class UpdateNewsDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Thumbnail { get; set; }
    public string? Author { get; set; }
    public bool? IsActive { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; }
}


// ==================== RBAC ====================
public class RoleDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    [ReferenceCollection(typeof(PermissionDto), "TblRolePermission", "RoleCode", "PermissionCode")]
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? PermissionCodes { get; set; }
}

public class PermissionDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string? Description { get; set; }
}

public class RolePermissionDto
{
    public string RoleCode { get; set; } = null!;
    public string PermissionCode { get; set; } = null!;
}
