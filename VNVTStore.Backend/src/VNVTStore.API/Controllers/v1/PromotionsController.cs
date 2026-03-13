using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Promotions.Commands;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class PromotionsController : BaseApiController<TblPromotion, PromotionDto, CreatePromotionDto, UpdatePromotionDto>
{
    private readonly ICurrentUser _currentUser;

    public PromotionsController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotions([FromQuery] GetPagedQuery<PromotionDto> query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    // Specific endpoints
    [HttpGet("flash-sale")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<PromotionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlashSales()
    {
        var result = await Mediator.Send(new GetFlashSaleQuery());
        return HandleResult(result);
    }
    
    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var result = await Mediator.Send(new ImportPromotionsCommand(memoryStream));
        return HandleResult(result);
    }
}
