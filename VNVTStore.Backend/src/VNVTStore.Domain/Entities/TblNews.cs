using System;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public partial class TblNews : IEntity
{
    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? Thumbnail { get; set; }

    public string? Author { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? Slug { get; set; }

    public string? ModifiedType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
