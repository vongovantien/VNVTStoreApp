using System;

namespace VNVTStore.Application.DTOs;

public class SystemSecretDto
{
    public string Code { get; set; } = null!;
    public string? SecretValue { get; set; }
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
