using MediatR;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.Coupons.Commands;
using VNVTStore.Application.Coupons.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[Route("api/v1/[controller]")]
[ApiController]
public class CouponsController : BaseApiController<CouponDto, CreateCouponDto, CouponDto>
{
    public CouponsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Validate a coupon code
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        var result = await Mediator.Send(new ValidateCouponCommand(request.CouponCode, request.OrderAmount));
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCoupons([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllCouponsQuery(pageIndex, pageSize));
        return HandleResult(result);
    }

    protected override IRequest<Result<PagedResult<CouponDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort)
        => new GetPagedQuery<CouponDto>(pageIndex, pageSize, search, sort);

    protected override IRequest<Result<CouponDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<CouponDto>(code);

    protected override IRequest<Result<CouponDto>> CreateCreateCommand(CreateCouponDto dto)
        => new CreateCommand<CreateCouponDto, CouponDto>(dto);

    protected override IRequest<Result<CouponDto>> CreateUpdateCommand(string code, CouponDto dto)
        => throw new NotImplementedException("Coupons do not support update.");

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblCoupon>(code);
}

public record ValidateCouponRequest(string CouponCode, decimal OrderAmount);
