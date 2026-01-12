using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Promotions.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities; // For TblPromotion marker

namespace VNVTStore.API.Controllers.v1;

public class PromotionsController : BaseApiController<PromotionDto, CreatePromotionDto, UpdatePromotionDto>
{
    private readonly ICurrentUser _currentUser;

    public PromotionsController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    // Factory methods for Generic Base Controller
    protected override IRequest<Result<PagedResult<PromotionDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort, List<SearchDTO>? filters)
    {
        return new GetPagedQuery<PromotionDto>(pageIndex, pageSize, search, sort, filters);
    }

    protected override IRequest<Result<PromotionDto>> CreateGetByCodeQuery(string code)
    {
        return new GetByCodeQuery<PromotionDto>(code);
    }

    protected override IRequest<Result<PromotionDto>> CreateCreateCommand(CreatePromotionDto dto)
    {
        return new CreateCommand<CreatePromotionDto, PromotionDto>(dto);
    }

    protected override IRequest<Result<PromotionDto>> CreateUpdateCommand(string code, UpdatePromotionDto dto)
    {
        return new UpdateCommand<UpdatePromotionDto, PromotionDto>(code, dto);
    }

    protected override IRequest<Result> CreateDeleteCommand(string code)
    {
        return new DeleteCommand<TblPromotion>(code);
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
}
