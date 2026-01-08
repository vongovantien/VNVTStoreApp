namespace VNVTStore.Application.DTOs;

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
    public DateTime? CreatedAt { get; set; }
    public string? Token { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
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
    public string? CategoryName { get; set; }
    public int? StockQuantity { get; set; }
    public string? Sku { get; set; }
    public decimal? Weight { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ProductImageDto> ProductImages { get; set; } = new();
}

public class ProductImageDto
{
    public string Code { get; set; } = null!;
    public string? ImageUrl { get; set; }
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
    public string? ImageUrl { get; set; }
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
    public string? Status { get; set; }
    public string? AddressCode { get; set; }
    public string? CouponCode { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public string Code { get; set; } = null!;
    public string OrderCode { get; set; } = null!;
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public decimal? DiscountAmount { get; set; }
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
    public string? UserName { get; set; }
}

// ==================== COUPON ====================
public class CouponDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string? PromotionCode { get; set; }
    public int? UsageCount { get; set; }
    public bool IsValid { get; set; }
}

// ==================== PROMOTION ====================
public class PromotionDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public bool? IsActive { get; set; }
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
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new(Enumerable.Empty<T>(), 0, pageNumber, pageSize);
}
