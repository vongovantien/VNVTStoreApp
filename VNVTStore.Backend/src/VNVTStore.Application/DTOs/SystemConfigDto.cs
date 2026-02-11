namespace VNVTStore.Application.DTOs
{
    public class SystemConfigDto
    {
        public string ConfigKey { get; set; } = null!;
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateSystemConfigDto
    {
        public string ConfigKey { get; set; } = null!;
        public string? ConfigValue { get; set; }
        public bool? IsActive { get; set; }
    }
}
