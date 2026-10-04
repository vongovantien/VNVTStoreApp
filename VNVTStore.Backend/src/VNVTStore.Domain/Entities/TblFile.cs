using System.ComponentModel.DataAnnotations.Schema;
using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblFile : BaseEntity
{
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
    public string Path { get; set; } = null!;
    public string? MasterCode { get; set; }
    public string? MasterType { get; set; }

    // No IsFixed/CreatedBy/UpdatedBy columns in DB yet
    [NotMapped] public new bool IsFixed { get; set; }
    [NotMapped] public new string? CreatedBy { get; set; }
    [NotMapped] public new string? UpdatedBy { get; set; }

    public static TblFile Create(string fileName, string originalName, string extension,
        string mimeType, long size, string path, string? masterCode = null, string? masterType = null)
    {
        return new TblFile
        {
            FileName     = fileName,
            OriginalName = originalName,
            Extension    = extension,
            MimeType     = mimeType,
            Size         = size,
            Path         = path,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow,
            MasterCode   = masterCode,
            MasterType   = masterType
        };
    }
}
