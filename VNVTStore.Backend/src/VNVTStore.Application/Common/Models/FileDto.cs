namespace VNVTStore.Application.Common.Models;

public class FileDto
{
    public string Code { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
    public string Path { get; set; } = null!;
    public string Url { get; set; } = null!; // For frontend usage
}
