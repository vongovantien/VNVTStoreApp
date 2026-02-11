using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Infrastructure.Authorization;

namespace VNVTStore.API.Controllers.v1;

public class PermissionsController : BaseApiController<TblPermission, PermissionDto, PermissionDto, PermissionDto>
{
    public PermissionsController(IMediator mediator) : base(mediator) { }
    
    // Usually permissions are read-only for the UI through this controller
    
    [HttpGet("all")]
    [HasPermission(Permissions.Settings.ManageRoles)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetPagedQuery<PermissionDto>(PageSize: 1000));
        return Ok(result);
    }
}
