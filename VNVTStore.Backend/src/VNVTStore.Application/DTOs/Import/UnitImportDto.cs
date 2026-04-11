using System.ComponentModel;

namespace VNVTStore.Application.DTOs.Import;

public class UnitImportDto
{
    [Description("Mã đơn vị tính")]
    public string Code { get; set; } = null!;

    [Description("Tên đơn vị tính")]
    public string Name { get; set; } = null!;

    [Description("Ký hiệu")]
    public string? Symbol { get; set; }

    [Description("Mô tả")]
    public string? Description { get; set; }

    [Description("Kích hoạt (true/false)")]
    public bool IsActive { get; set; }
}
