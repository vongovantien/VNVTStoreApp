using Microsoft.AspNetCore.Http;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<TblAuditLog> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        IRepository<TblAuditLog> repository, 
        IUnitOfWork unitOfWork, 
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string? target = null, string? detail = null)
    {
        var log = new TblAuditLog
        {
            Code = Guid.NewGuid().ToString("N"),
            UserCode = _currentUser.UserCode,
            Action = action,
            Target = target,
            Detail = detail,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ModifiedType = "ADD"
        };

        await _repository.AddAsync(log, default);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(SearchParams searchParams)
    {
        // For now, simple list
        var logs = await _repository.GetAllAsync();
        // Mapping would be needed if using AutoMapper here, but for now we follow the pattern
        return new PagedResult<AuditLogDto>(new List<AuditLogDto>(), 0, 1, 10);
    }
}
