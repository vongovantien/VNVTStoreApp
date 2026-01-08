namespace VNVTStore.Domain.Enums;

/// <summary>
/// Trạng thái đơn hàng
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

/// <summary>
/// Trạng thái thanh toán
/// </summary>
public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    Cancelled
}

/// <summary>
/// Phương thức thanh toán
/// </summary>
public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    BankTransfer,
    COD,
    EWallet,
    PayPal
}

/// <summary>
/// Vai trò người dùng
/// </summary>
public enum UserRole
{
    Customer,
    Admin
}

/// <summary>
/// Trạng thái người dùng
/// </summary>
public enum UserStatus
{
    Active,
    Inactive,
    Banned,
    Pending
}
