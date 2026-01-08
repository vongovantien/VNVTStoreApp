using Microsoft.AspNetCore.Http;

namespace VNVTStore.Application.Localization;

/// <summary>
/// Service to get localized messages
/// </summary>
public interface ILocalizationService
{
    string GetMessage(string key);
    string GetMessage(string key, params object[] args);
}

/// <summary>
/// Implementation of localization service
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private static readonly Dictionary<string, Dictionary<string, string>> _messages = new()
    {
        ["vi"] = new Dictionary<string, string>
        {
            // Common
            ["Success"] = "Thành công",
            ["Error"] = "Có lỗi xảy ra",
            ["NotFound"] = "Không tìm thấy",
            ["Unauthorized"] = "Không có quyền truy cập",
            ["ValidationError"] = "Dữ liệu không hợp lệ",
            
            // Auth
            ["Auth.LoginSuccess"] = "Đăng nhập thành công",
            ["Auth.LoginFailed"] = "Sai email hoặc mật khẩu",
            ["Auth.RegisterSuccess"] = "Đăng ký thành công",
            ["Auth.UserExists"] = "Email đã được sử dụng",
            ["Auth.InvalidToken"] = "Token không hợp lệ",
            ["Auth.TokenExpired"] = "Token đã hết hạn",
            
            // Product
            ["Product.Created"] = "Tạo sản phẩm thành công",
            ["Product.Updated"] = "Cập nhật sản phẩm thành công",
            ["Product.Deleted"] = "Xóa sản phẩm thành công",
            ["Product.NotFound"] = "Không tìm thấy sản phẩm",
            ["Product.CodeExists"] = "Mã sản phẩm đã tồn tại",
            
            // Category
            ["Category.Created"] = "Tạo danh mục thành công",
            ["Category.Updated"] = "Cập nhật danh mục thành công",
            ["Category.Deleted"] = "Xóa danh mục thành công",
            ["Category.NotFound"] = "Không tìm thấy danh mục",
            
            // Order
            ["Order.Created"] = "Tạo đơn hàng thành công",
            ["Order.Updated"] = "Cập nhật đơn hàng thành công",
            ["Order.Cancelled"] = "Hủy đơn hàng thành công",
            ["Order.NotFound"] = "Không tìm thấy đơn hàng",
        },
        ["en"] = new Dictionary<string, string>
        {
            // Common
            ["Success"] = "Success",
            ["Error"] = "An error occurred",
            ["NotFound"] = "Not found",
            ["Unauthorized"] = "Unauthorized access",
            ["ValidationError"] = "Validation error",
            
            // Auth
            ["Auth.LoginSuccess"] = "Login successful",
            ["Auth.LoginFailed"] = "Invalid email or password",
            ["Auth.RegisterSuccess"] = "Registration successful",
            ["Auth.UserExists"] = "Email already registered",
            ["Auth.InvalidToken"] = "Invalid token",
            ["Auth.TokenExpired"] = "Token has expired",
            
            // Product
            ["Product.Created"] = "Product created successfully",
            ["Product.Updated"] = "Product updated successfully",
            ["Product.Deleted"] = "Product deleted successfully",
            ["Product.NotFound"] = "Product not found",
            ["Product.CodeExists"] = "Product code already exists",
            
            // Category
            ["Category.Created"] = "Category created successfully",
            ["Category.Updated"] = "Category updated successfully",
            ["Category.Deleted"] = "Category deleted successfully",
            ["Category.NotFound"] = "Category not found",
            
            // Order
            ["Order.Created"] = "Order created successfully",
            ["Order.Updated"] = "Order updated successfully",
            ["Order.Cancelled"] = "Order cancelled successfully",
            ["Order.NotFound"] = "Order not found",
        }
    };

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentLanguage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue("Language", out var lang) == true && lang is string language)
        {
            return language;
        }
        return "vi"; // Default
    }

    public string GetMessage(string key)
    {
        var lang = GetCurrentLanguage();
        
        if (_messages.TryGetValue(lang, out var langMessages) && 
            langMessages.TryGetValue(key, out var message))
        {
            return message;
        }
        
        // Fallback to Vietnamese
        if (_messages["vi"].TryGetValue(key, out var fallbackMessage))
        {
            return fallbackMessage;
        }
        
        return key; // Return key if no message found
    }

    public string GetMessage(string key, params object[] args)
    {
        var message = GetMessage(key);
        return string.Format(message, args);
    }
}
