namespace VNVTStore.Application.DTOs;

public class TagDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = null!;
}

public class UpdateTagDto
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
