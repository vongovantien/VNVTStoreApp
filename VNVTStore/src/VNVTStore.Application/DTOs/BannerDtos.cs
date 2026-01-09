namespace VNVTStore.Application.DTOs;

public class BannerDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBannerDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
}

public class UpdateBannerDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
    public bool? IsActive { get; set; }
    public int? Priority { get; set; }
}
