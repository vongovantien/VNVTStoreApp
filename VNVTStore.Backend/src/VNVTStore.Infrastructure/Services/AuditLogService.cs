using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string? target = null, string? detail = null)
    {
        var log = new TblAuditLog
        {
            Code = Guid.NewGuid().ToString(),
            UserCode = _currentUser.UserCode,
            Action = action,
            Target = target,
            Detail = detail,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            ModifiedType = "ADD"
        };

        _context.TblAuditLogs.Add(log);
        await _context.SaveChangesAsync(default);
    }

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(SearchParams paramsObj)
    {
        var query = _context.TblAuditLogs
            .Include(l => l.UserCodeNavigation)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(paramsObj.Searching))
        {
            var search = paramsObj.Searching.ToLower();
            query = query.Where(l => 
                l.Action.ToLower().Contains(search) || 
                (l.Target != null && l.Target.ToLower().Contains(search)) ||
                (l.Detail != null && l.Detail.ToLower().Contains(search)) ||
                (l.UserCodeNavigation != null && l.UserCodeNavigation.FullName != null && l.UserCodeNavigation.FullName.ToLower().Contains(search))
            );
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((paramsObj.PageIndex - 1) * paramsObj.PageSize)
            .Take(paramsObj.PageSize)
            .Select(l => new AuditLogDto
            {
                Code = l.Code,
                UserCode = l.UserCode,
                UserName = l.UserCodeNavigation != null ? l.UserCodeNavigation.FullName : "System",
                Action = l.Action,
                Target = l.Target,
                Detail = l.Detail,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt,
                IsActive = l.IsActive
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>(items, total, paramsObj.PageIndex, paramsObj.PageSize);
    }
}
