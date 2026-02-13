using System.Threading.Tasks;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string? target = null, string? detail = null);
    Task<PagedResult<AuditLogDto>> GetLogsAsync(SearchParams searchParams);
}
