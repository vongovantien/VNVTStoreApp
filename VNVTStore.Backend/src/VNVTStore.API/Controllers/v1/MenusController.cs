using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.API.Controllers;

namespace VNVTStore.API.Controllers.v1;

public class MenusController : BaseApiController<TblMenu, MenuDto, MenuDto, MenuDto>
{
    public MenusController(IMediator mediator) : base(mediator) { }
    
    // Inherits standard CRUD and Search from BaseApiController
}
