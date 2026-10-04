using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

[Table("TblBanner")]
public class TblBanner : BaseEntity
{
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Content { get; set; }

    [MaxLength(200)]
    public string? LinkUrl { get; set; }

    [MaxLength(150)]
    public string? LinkText { get; set; }

    public int Priority { get; set; } = 0;

    // No IsFixed/CreatedBy/UpdatedBy columns for TblBanner in DB yet
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public void Update(string title, string? content, string? linkUrl, string? linkText, bool isActive, int priority)
    {
        Title    = title;
        Content  = content;
        LinkUrl  = linkUrl;
        LinkText = linkText;
        IsActive = isActive;
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }
}
