using System.Collections.Generic;
using System.Globalization;

namespace VNVTStore.Application.Common;

public static class MessageConstants
{
    public const string Success = "Common.Success";
    public const string Created = "Common.Created";
    public const string Updated = "Common.Updated";
    public const string Deleted = "Common.Deleted";
    public const string NotFound = "Common.NotFound";
    public const string BadRequest = "Common.BadRequest";
    public const string Unauthorized = "Common.Unauthorized";
    public const string Forbidden = "Common.Forbidden";
    public const string InternalError = "Common.InternalError";

    // Entities
    public const string Product = "Entity.Product";
    public const string Category = "Entity.Category";
    public const string Order = "Entity.Order";
    public const string Payment = "Entity.Payment";
    public const string Address = "Entity.Address";
    public const string Review = "Entity.Review";
    public const string Banner = "Entity.Banner";
    public const string User = "Entity.User";
    public const string Coupon = "Entity.Coupon";
    public const string Supplier = "Entity.Supplier";

    // Generic Error Keys
    public const string EntityNotFound = "Error.EntityNotFound";
    public const string AlreadyExists = "Error.AlreadyExists";
    public const string ValidationFailed = "Error.ValidationFailed";
    public const string Conflict = "Error.Conflict";

    // Specific messages
    public const string ProfileUpdated = "User.ProfileUpdated";
    public const string PasswordChanged = "User.PasswordChanged";
    public const string CurrentPasswordIncorrect = "User.CurrentPasswordIncorrect";
    public const string EmailInUse = "User.EmailInUse";
    public const string LoginSuccess = "Auth.LoginSuccess";
    public const string RegisterSuccess = "Auth.RegisterSuccess";
    public const string CartRetrieved = "Cart.Retrieved";
    public const string CartAdded = "Cart.Added";
    public const string CartUpdated = "Cart.Updated";
    public const string CartRemoved = "Cart.Removed";
    public const string OrderCreated = "Order.Created";
    public const string OrderRetrieved = "Order.Retrieved";
    public const string OrderCancelled = "Order.Cancelled";
    public const string OrderCannotCancel = "Order.CannotCancel";
    public const string CartEmpty = "Order.CartEmpty";
    public const string OrderItem = "Entity.OrderItem";
    public const string InvalidCredentials = "Auth.InvalidCredentials";
    public const string UsernameExists = "Auth.UsernameExists";
    public const string InsufficientStock = "Product.InsufficientStock";
    public const string ReviewAlreadyExists = "Review.AlreadyExists";
    public const string ReviewForbidden = "Review.Forbidden";
    public const string CouponNotActive = "Coupon.NotActive";
    public const string CouponExpired = "Coupon.Expired";
    public const string CouponLimitReached = "Coupon.LimitReached";
    public const string CouponNotFound = "Coupon.NotFound";
    public const string CouponMinOrderAmountNotMet = "Coupon.MinOrderAmountNotMet";
    public const string CategoryHasProducts = "Category.HasProducts";

    public const string NotificationNewOrder = "Notification.NewOrder";
    public const string NotificationSystemBroadcast = "Notification.SystemBroadcast";

    public const string RequireLoginOrGuestInfo = "Auth.RequireLoginOrGuestInfo";
    
    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["vi"] = new()
        {
            [Success] = "Thành công",
            [Created] = "Tạo mới thành công",
            [Updated] = "Cập nhật thành công",
            [Deleted] = "Xóa thành công",
            [NotFound] = "Không tìm thấy dữ liệu",
            [BadRequest] = "Yêu cầu không hợp lệ",
            [Unauthorized] = "Chưa xác thực",
            [Forbidden] = "Không có quyền truy cập",
            [InternalError] = "Lỗi hệ thống",
            [Product] = "Sản phẩm",
            [Category] = "Danh mục",
            [Order] = "Đơn hàng",
            [Payment] = "Thanh toán",
            [Address] = "Địa chỉ",
            [Review] = "Đánh giá",
            [User] = "Người dùng",
            [Coupon] = "Mã giảm giá",
            [Supplier] = "Nhà cung cấp",
            [EntityNotFound] = "{0} không tìm thấy với mã '{1}'",
            [AlreadyExists] = "{0} '{1}' đã tồn tại",
            [ValidationFailed] = "Dữ liệu không hợp lệ: {0}",
            [Conflict] = "Xung đột dữ liệu: {0}",
            [ProfileUpdated] = "Cập nhật thông tin cá nhân thành công",
            [PasswordChanged] = "Đổi mật khẩu thành công",
            [CurrentPasswordIncorrect] = "Mật khẩu hiện tại không chính xác",
            [EmailInUse] = "Email đã được sử dụng",
            [LoginSuccess] = "Đăng nhập thành công",
            [RegisterSuccess] = "Đăng ký tài khoản thành công",
            [CartRetrieved] = "Lấy thông tin giỏ hàng thành công",
            [CartAdded] = "Đã thêm vào giỏ hàng",
            [CartUpdated] = "Đã cập nhật giỏ hàng",
            [CartRemoved] = "Đã xóa khỏi giỏ hàng",
            [OrderCreated] = "Đặt hàng thành công",
            [OrderRetrieved] = "Lấy thông tin đơn hàng thành công",
            [OrderCancelled] = "Đã hủy đơn hàng",
            [OrderCannotCancel] = "Chỉ có thể hủy đơn hàng đang chờ xử lý",
            [CartEmpty] = "Giỏ hàng đang trống",
            [OrderItem] = "Sản phẩm trong đơn hàng",
            [InvalidCredentials] = "Tên đăng nhập hoặc mật khẩu không chính xác",
            [UsernameExists] = "Tên đăng nhập đã tồn tại",
            [InsufficientStock] = "Sản phẩm '{0}' không đủ hàng",
            [ReviewAlreadyExists] = "Bạn đã đánh giá sản phẩm này rồi",
            [ReviewForbidden] = "Không có quyền thực hiện đánh giá này",
            [CouponNotActive] = "Mã giảm giá không hoạt động",
            [CouponExpired] = "Mã giảm giá đã hết hạn hoặc chưa bắt đầu",
            [CouponLimitReached] = "Mã giảm giá đã hết lượt sử dụng",
            [CouponNotFound] = "Không tìm thấy mã giảm giá",
    [CouponMinOrderAmountNotMet] = "Giá trị đơn hàng chưa đạt mức tối thiểu để áp dụng mã này",
            [CategoryHasProducts] = "Không thể xóa danh mục '{0}' vì đang có {1} sản phẩm. Vui lòng chuyển sản phẩm sang danh mục khác trước.",
            [NotificationNewOrder] = "Đơn hàng mới: {0}",
            [NotificationSystemBroadcast] = "Thông báo hệ thống: {0}",
            [RequireLoginOrGuestInfo] = "Vui lòng đăng nhập hoặc điền thông tin liên hệ."
        },
        ["en"] = new()
        {
            [Success] = "Success",
            [Created] = "Created successfully",
            [Updated] = "Updated successfully",
            [Deleted] = "Deleted successfully",
            [NotFound] = "Resource not found",
            [BadRequest] = "Bad request",
            [Unauthorized] = "Unauthorized",
            [Forbidden] = "Forbidden",
            [InternalError] = "Internal server error",
            [Product] = "Product",
            [Category] = "Category",
            [Order] = "Order",
            [Payment] = "Payment",
            [Address] = "Address",
            [Review] = "Review",
            [User] = "User",
            [Coupon] = "Coupon",
            [Supplier] = "Supplier",
            [EntityNotFound] = "{0} with id/code '{1}' not found",
            [AlreadyExists] = "{0} '{1}' already exists",
            [ValidationFailed] = "Validation failed: {0}",
            [Conflict] = "Conflict: {0}",
            [ProfileUpdated] = "Profile updated successfully",
            [PasswordChanged] = "Password changed successfully",
            [CurrentPasswordIncorrect] = "Current password is incorrect",
            [EmailInUse] = "Email already in use",
            [LoginSuccess] = "Login successful",
            [RegisterSuccess] = "User registered successfully",
            [CartRetrieved] = "Cart retrieved successfully",
            [CartAdded] = "Item added to cart",
            [CartUpdated] = "Cart updated",
            [CartRemoved] = "Item removed from cart",
            [OrderCreated] = "Order created successfully",
            [OrderRetrieved] = "Order retrieved successfully",
            [OrderCancelled] = "Order cancelled successfully",
            [OrderCannotCancel] = "Can only cancel pending orders",
            [CartEmpty] = "Cart is empty",
            [OrderItem] = "Order Item",
            [InvalidCredentials] = "Invalid username or password",
            [UsernameExists] = "Username already exists",
            [InsufficientStock] = "Product '{0}' insufficient stock",
            [ReviewAlreadyExists] = "You have already reviewed this item",
            [ReviewForbidden] = "Forbidden to review this item",
            [CouponNotActive] = "Coupon is not active",
            [CouponExpired] = "Coupon has expired or not yet started",
            [CouponLimitReached] = "Coupon usage limit reached",
            [CouponNotFound] = "Coupon not found",
    [CouponMinOrderAmountNotMet] = "Minimum order amount for this coupon not reached",
            [CategoryHasProducts] = "Cannot delete category '{0}' because it has {1} products. Please move products to another category first.",
            [NotificationNewOrder] = "New Order: {0}",
            [NotificationSystemBroadcast] = "System Notification: {0}",
            [RequireLoginOrGuestInfo] = "Please login or provide contact information."
        }
    };

    public static string Get(string key, params object[] args)
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        if (!Messages.ContainsKey(culture)) culture = "vi"; // Default to Vietnamese
        
        var message = Messages[culture].GetValueOrDefault(key, key);
        return args.Length > 0 ? string.Format(message, args) : message;
    }
}
