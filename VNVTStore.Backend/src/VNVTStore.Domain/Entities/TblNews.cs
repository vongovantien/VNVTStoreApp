using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblNews : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Thumbnail { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; }
}
