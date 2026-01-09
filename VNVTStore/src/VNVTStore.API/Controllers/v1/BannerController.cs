using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class BannerController : BaseApiController
{
    public BannerController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetBanners([FromQuery] GetPagedQuery<BannerDto> query)
    {
        return HandleResult<PagedResult<BannerDto>>(await Mediator.Send(query));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetBanner(string code)
    {
        return HandleResult<BannerDto>(await Mediator.Send(new GetByCodeQuery<BannerDto>(code)));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerDto dto)
    {
        return HandleResult<BannerDto>(await Mediator.Send(new CreateCommand<CreateBannerDto, BannerDto>(dto)));
    }

    [HttpPut("{code}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateBanner(string code, [FromBody] UpdateBannerDto dto)
    {
        return HandleResult<BannerDto>(await Mediator.Send(new UpdateCommand<UpdateBannerDto, BannerDto>(code, dto)));
    }

    [HttpDelete("{code}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteBanner(string code)
    {
        return HandleDelete(await Mediator.Send(new DeleteCommand<TblBanner>(code)));
    }
}
