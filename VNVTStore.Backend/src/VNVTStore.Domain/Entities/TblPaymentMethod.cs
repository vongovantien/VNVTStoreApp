using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

/// <summary>
/// Danh mục phương thức thanh toán — quản lý từ Admin, thay thế enum cứng.
/// IsFixed = true cho các phương thức hệ thống (COD, BankTransfer...) không được xóa.
/// </summary>
public class TblPaymentMethod : BaseEntity
{
    /// <summary>Tên hiển thị, e.g. "Thanh toán khi nhận hàng (COD)"</summary>
    public string Name { get; set; } = null!;

    /// <summary>Mô tả ngắn</summary>
    public string? Description { get; set; }

    /// <summary>URL icon hoặc tên icon lucide</summary>
    public string? IconUrl { get; set; }

    /// <summary>Thứ tự hiển thị ở checkout</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>Phương thức có hỗ trợ thanh toán online không (cần gateway)</summary>
    public bool IsOnline { get; set; } = false;

    public static TblPaymentMethod Create(string code, string name, string? description = null,
        string? iconUrl = null, int sortOrder = 0, bool isOnline = false, bool isFixed = false)
    {
        return new TblPaymentMethod
        {
            Code        = code,
            Name        = name,
            Description = description,
            IconUrl     = iconUrl,
            SortOrder   = sortOrder,
            IsOnline    = isOnline,
            IsActive    = true,
            IsFixed     = isFixed,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow,
            ModifiedType = "ADD"
        };
    }
}
