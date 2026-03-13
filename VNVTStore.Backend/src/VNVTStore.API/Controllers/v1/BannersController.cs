using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

public class BannersController : BaseApiController<TblBanner, BannerDto, CreateBannerDto, UpdateBannerDto>
{
    public BannersController(IMediator mediator) : base(mediator)
    {
    }

    // Keep this for GET /api/v1/banners (simple list via query params)
    [HttpGet]
    public async Task<IActionResult> GetBanners([FromQuery] GetPagedQuery<BannerDto> query)
    {
        return HandleResult<PagedResult<BannerDto>>(await Mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public override Task<IActionResult> Create([FromBody] RequestDTO<CreateBannerDto> request) => base.Create(request);

    [HttpPut("{code}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public override Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateBannerDto> request) => base.Update(code, request);

    [HttpDelete("{code}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public override Task<IActionResult> Delete(string code) => base.Delete(code);
}
