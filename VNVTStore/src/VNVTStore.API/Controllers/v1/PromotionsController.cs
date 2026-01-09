using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class PromotionsController : BaseApiController<PromotionDto, CreatePromotionDto, UpdatePromotionDto>
{
    public PromotionsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get active promotions (Public)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePromotions()
    {
        var result = await Mediator.Send(new GetActivePromotionsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Get all promotions (Admin)
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllPromotions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        var result = await Mediator.Send(new GetAllPromotionsQuery(pageIndex, pageSize, isActive));
        return HandleResult(result);
    }

    protected override IRequest<Result<PagedResult<PromotionDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort)
        => new GetPagedQuery<PromotionDto>(pageIndex, pageSize, search, sort);

    protected override IRequest<Result<PromotionDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<PromotionDto>(code);

    protected override IRequest<Result<PromotionDto>> CreateCreateCommand(CreatePromotionDto dto)
        => new CreateCommand<CreatePromotionDto, PromotionDto>(dto);

    protected override IRequest<Result<PromotionDto>> CreateUpdateCommand(string code, UpdatePromotionDto dto)
        => new UpdateCommand<UpdatePromotionDto, PromotionDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblPromotion>(code);
}
