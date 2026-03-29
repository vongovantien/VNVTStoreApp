using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Commands;
using VNVTStore.Application.Common.Queries;
using VNVTStore.Application.DTOs;

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
