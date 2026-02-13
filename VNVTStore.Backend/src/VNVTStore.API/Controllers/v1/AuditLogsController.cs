using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Common;

namespace VNVTStore.API.Controllers.v1;

[Authorize(Roles = "admin")]
[Route("api/v1/[controller]")]
public class AuditLogsController : BaseApiController<TblAuditLog, AuditLogDto, AuditLogDto, AuditLogDto>
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService, IMediator mediator) : base(mediator)
    {
        _auditLogService = auditLogService;
    }

    // BaseController already implements [HttpPost("search")] which calls GetPagedQuery<AuditLogDto>

    // Disable Create/Update/Delete for Audit Logs
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] RequestDTO<AuditLogDto> request)
    {
        return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }

    [HttpPut("{code}")]
    public override Task<IActionResult> Update(string code, [FromBody] RequestDTO<AuditLogDto> request)
    {
        return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }

    [HttpDelete("{code}")]
    public override Task<IActionResult> Delete(string code)
    {
        return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }
}
