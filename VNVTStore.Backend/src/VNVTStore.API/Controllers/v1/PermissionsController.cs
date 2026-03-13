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
}
