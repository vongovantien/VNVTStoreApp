using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Commands;
using VNVTStore.Application.Common.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Infrastructure.Persistence;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/configs")]
public class ConfigsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConfigsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("contacts")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Dictionary<string, string>>>> GetPublicContacts([FromServices] ApplicationDbContext context)
    {
        var contactCodes = new List<string> 
        { 
            "CONTACT_PHONE", 
            "CONTACT_FACEBOOK", 
            "CONTACT_TIKTOK", 
            "CONTACT_MESSENGER", 
            "CONTACT_ZALO", 
            "CONTACT_MAPS", 
            "CONTACT_ADDRESS",
            "ADMIN_EMAIL"
        };
        
        var secrets = await context.TblSystemSecrets
            .Where(s => s.IsActive && contactCodes.Contains(s.Code))
            .ToDictionaryAsync(s => s.Code, s => s.SecretValue ?? string.Empty);
            
        return Ok(ApiResponse<Dictionary<string, string>>.Ok(secrets));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ShopConfigDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetShopConfigsQuery());
        return Ok(ApiResponse<List<ShopConfigDto>>.Ok(result.Value ?? new List<ShopConfigDto>()));
    }

    [HttpGet("{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ShopConfigDto>>> GetByCode(string code)
    {
        var result = await _mediator.Send(new GetConfigByCodeQuery(code));
        if (result.IsFailure) return NotFound(result.Error);
        return Ok(ApiResponse<ShopConfigDto>.Ok(result.Value!));
    }

    [HttpPut("{code}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse<ShopConfigDto>>> Update(string code, UpdateConfigDto dto)
    {
        var result = await _mediator.Send(new UpdateConfigCommand(code, dto));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(ApiResponse<ShopConfigDto>.Ok(result.Value!));
    }
}
