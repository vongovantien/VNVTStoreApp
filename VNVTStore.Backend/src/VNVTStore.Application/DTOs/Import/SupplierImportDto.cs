using System.ComponentModel;

namespace VNVTStore.Application.DTOs.Import;

public class SupplierImportDto
{
    [Description("Mã nhà cung cấp")]
    public string Code { get; set; } = null!;

    [Description("Tên nhà cung cấp")]
    public string Name { get; set; } = null!;

    [Description("Người liên hệ")]
    public string? ContactPerson { get; set; }

    [Description("Email")]
    public string? Email { get; set; }

    [Description("Số điện thoại")]
    public string? Phone { get; set; }

    [Description("Địa chỉ")]
    public string? Address { get; set; }

    [Description("Mã số thuế")]
    public string? TaxCode { get; set; }

    [Description("Ghi chú")]
    public string? Notes { get; set; }

    [Description("Kích hoạt (true/false)")]
    public bool IsActive { get; set; }
}
