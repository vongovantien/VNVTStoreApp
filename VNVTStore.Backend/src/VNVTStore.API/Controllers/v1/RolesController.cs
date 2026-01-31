using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Infrastructure.Authorization;

namespace VNVTStore.API.Controllers.v1;

public class RolesController : BaseApiController<TblRole, RoleDto, CreateRoleDto, UpdateRoleDto>
{
    public RolesController(IMediator mediator) : base(mediator) { }
    
    // Inherits all standard CRUD operations from BaseApiController
    
    [HttpGet("permissions")]
    [HasPermission(Permissions.Settings.ManageRoles)]
    public async Task<IActionResult> GetPermissions()
    {
        // For now, return permissions from TblPermission via MediatR if we add a query for it, 
        // or just use generic GetPaged for TblPermission.
        return Ok(await Mediator.Send(new GetPagedQuery<PermissionDto>()));
    }
}
