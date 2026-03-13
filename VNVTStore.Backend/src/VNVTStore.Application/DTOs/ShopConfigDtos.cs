using VNVTStore.Application.Common;

namespace VNVTStore.Application.DTOs;

public class ShopConfigDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
}

public class NotificationDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsRead { get; set; }
    public string? Link { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class UpdateConfigDto
{
    public string? ConfigValue { get; set; }
    public string? Description { get; set; }
}
