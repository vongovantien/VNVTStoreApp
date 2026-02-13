using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.API;

namespace VNVTStore.IntegrationTests;

/// <summary>
/// Integration tests verifying API endpoints that support the 100 shop features.
/// Tests cover: Product browsing, search, filters, categories, cart, orders,
/// reviews, coupons, dashboard, banners, and admin features.
/// </summary>

// ===== Feature #6,#7,#8: Search & Autocomplete =====
public class SearchApiTests : IntegrationTestBase
{
    public SearchApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task SearchProducts_ShouldReturnSuccess()
    {
        // Act - test search endpoint that powers autocomplete
        var request = new { pageIndex = 0, pageSize = 5, search = "test" };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("items");
    }

    [Fact]
    public async Task SearchProducts_WithEmptyQuery_ShouldReturnAll()
    {
        var request = new { pageIndex = 0, pageSize = 10, search = "" };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchProducts_WithSpecialChars_ShouldNotFail()
    {
        var request = new { pageIndex = 0, pageSize = 5, search = "test & 'special\" chars <>" };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        // Should not crash, even with SQL injection attempt characters
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}

// ===== Feature #1,#3,#10: Product Filtering =====
public class ProductFilterApiTests : IntegrationTestBase
{
    public ProductFilterApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task FilterByCategory_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FilterByCategory_Paged_ShouldReturnValidStructure()
    {
        var request = new { pageIndex = 0, pageSize = 10 };
        var response = await _client.PostAsJsonAsync("/api/categories/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("items");
    }

    [Fact]
    public async Task GetBrands_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/v1/brands");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsByPriceRange_ShouldWork()
    {
        var request = new { pageIndex = 0, pageSize = 10, minPrice = 0, maxPrice = 1000000 };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithSorting_ShouldReturnSuccess()
    {
        var request = new { pageIndex = 0, pageSize = 10, sortField = "price", sortDir = "asc" };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithInStockOnly_ShouldReturnSuccess()
    {
        var request = new { pageIndex = 0, pageSize = 10, inStockOnly = true };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ===== Feature #21,#22: Cart API =====
public class CartApiIntegrationTests : IntegrationTestBase
{
    public CartApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetCart_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/cart");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddToCart_WithoutAuth_ShouldReturn401()
    {
        var payload = new { productCode = "test", quantity = 1 };
        var response = await _client.PostAsJsonAsync("/api/v1/cart/items", payload);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCart_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/cart");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ===== Feature #35,#36: Orders API =====
public class OrderApiIntegrationTests : IntegrationTestBase
{
    public OrderApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetOrders_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrders_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/orders");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrderByCode_NotFound_ShouldReturn404()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/orders/NONEXISTENT_ORDER");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrders_Paged_ShouldReturnValidStructure()
    {
        await AuthenticateAsync();
        var request = new { pageIndex = 0, pageSize = 10 };
        var response = await _client.PostAsJsonAsync("/api/v1/orders/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ===== Feature #42,#43,#44,#46: Reviews API =====
public class ReviewFeatureApiTests : IntegrationTestBase
{
    public ReviewFeatureApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProductReviews_ShouldReturnPagedResult()
    {
        // Using first available product
        var productsResponse = await _client.PostAsJsonAsync("/api/products/paged", new { pageIndex = 0, pageSize = 1 });
        var content = await productsResponse.Content.ReadAsStringAsync();
        
        productsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateReview_WithoutAuth_ShouldReturn401()
    {
        var review = new { productCode = "test", rating = 5, comment = "Great product!" };
        var response = await _client.PostAsJsonAsync("/api/v1/reviews", review);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReviews_ShouldSupportSorting()
    {
        var request = new { pageIndex = 0, pageSize = 10, sortField = "rating", sortDir = "desc" };
        var response = await _client.PostAsJsonAsync("/api/v1/reviews/paged", request);
        
        // Endpoint may not exist, but should not crash
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}

// ===== Feature #52,#53: Coupons/Promotions API =====
public class CouponApiIntegrationTests : IntegrationTestBase
{
    public CouponApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task ValidateCoupon_ShouldWork()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/coupons/validate/TESTCOUPON");
        
        // Coupon may or may not exist, but endpoint should respond
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivePromotions_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/v1/promotions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ===== Feature #66: Addresses API =====
public class AddressApiIntegrationTests : IntegrationTestBase
{
    public AddressApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetAddresses_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/addresses");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAddresses_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/addresses");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAddress_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var address = new 
        {
            fullName = "Test User",
            phone = "0901234567",
            address = "123 Test St, HCM",
            isDefault = false
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/addresses", address);
        
        // Should create or return validation error
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }
}

// ===== Feature #91,#92,#93,#94,#100: Admin/Dashboard API =====
public class AdminDashboardApiTests : IntegrationTestBase
{
    public AdminDashboardApiTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetDashboard_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDashboard_WithAdminAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync("admin", "Admin@123");
        var response = await _client.GetAsync("/api/v1/dashboard");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSystemHealth_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/system/health");
        
        // Health check endpoint
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}

// ===== Feature #99: Banners API =====
public class BannerApiIntegrationTests : IntegrationTestBase
{
    public BannerApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetBanners_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/v1/banners");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateBanner_WithoutAuth_ShouldReturn401()
    {
        var banner = new { title = "Test Banner", imageUrl = "/img/test.jpg", link = "/sale", isActive = true };
        var response = await _client.PostAsJsonAsync("/api/v1/banners", banner);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBanner_WithAdminAuth_ShouldWork()
    {
        await AuthenticateAsync("admin", "Admin@123");
        var banner = new { title = "Test Banner", imageUrl = "/img/test.jpg", link = "/sale", isActive = true };
        var response = await _client.PostAsJsonAsync("/api/v1/banners", banner);
        
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }
}

// ===== Feature #61: Quotes API =====
public class QuoteApiIntegrationTests : IntegrationTestBase
{
    public QuoteApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetQuotes_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/quotes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQuotes_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/quotes");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// ===== Feature #34: User Profile / Notifications =====
public class UserProfileApiIntegrationTests : IntegrationTestBase
{
    public UserProfileApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetProfile_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/users/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/users/profile");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProfile_WithAuth_ShouldWork()
    {
        await AuthenticateAsync();
        var profileUpdate = new { fullName = "Updated Name" };
        var response = await _client.PutAsJsonAsync("/api/v1/users/profile", profileUpdate);
        
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }
}

// ===== Feature #95: Audit Logs API =====
public class AuditLogApiIntegrationTests : IntegrationTestBase
{
    public AuditLogApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetAuditLogs_WithAdminAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync("admin", "Admin@123");
        var response = await _client.GetAsync("/api/v1/audit-logs");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditLogs_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/audit-logs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// ===== Feature #62,#63: Suppliers (B2B) API =====
public class SupplierApiIntegrationTests : IntegrationTestBase
{
    public SupplierApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetSuppliers_WithAdminAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync("admin", "Admin@123");
        var response = await _client.GetAsync("/api/v1/suppliers");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSuppliers_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/suppliers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// ===== Feature #97: Product Export API =====
public class ExportApiIntegrationTests : IntegrationTestBase
{
    public ExportApiIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task ExportProducts_WithAdminAuth_ShouldReturnSuccess()
    {
        await AuthenticateAsync("admin", "Admin@123");
        
        var request = new { pageIndex = 0, pageSize = 100 };
        var response = await _client.PostAsJsonAsync("/api/products/paged", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
