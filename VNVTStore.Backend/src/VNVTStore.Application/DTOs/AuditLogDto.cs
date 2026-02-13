using System;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.DTOs;

public class AuditLogDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string? UserCode { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = null!;
    public string? Target { get; set; }
    public string? Detail { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class SearchParams
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Searching { get; set; }
}

public class CreateAuditLogDto
{
    public string? Action { get; set; }
    public string? Target { get; set; }
    public string? Detail { get; set; }
}
