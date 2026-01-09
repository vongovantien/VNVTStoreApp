using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Addresses.Commands;
using VNVTStore.Application.Addresses.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : BaseApiController<AddressDto, CreateAddressDto, UpdateAddressDto>
{
    private readonly ICurrentUser _currentUser;

    public AddressesController(IMediator mediator, ICurrentUser currentUser) : base(mediator)
    {
        _currentUser = currentUser;
    }

    private string GetUserCode() => _currentUser.UserCode ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Get all addresses for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await Mediator.Send(new GetAllQuery<AddressDto>()); // Handler uses ICurrentUser to filter
        return HandleResult(result);
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] RequestDTO<CreateAddressDto> request)
    {
        request.PostObject!.UserCode = GetUserCode();
        return await base.Create(request);
    }

    [HttpPut("{code}")]
    public override async Task<IActionResult> Update(string code, [FromBody] RequestDTO<UpdateAddressDto> request)
    {
        return await base.Update(code, request);
    }

    [HttpDelete("{code}")]
    public override async Task<IActionResult> Delete(string code)
    {
        return await base.Delete(code);
    }

    /// <summary>
    /// Set address as default
    /// </summary>
    [HttpPost("{code}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(string code)
    {
        var result = await Mediator.Send(new SetDefaultAddressCommand(code));
        if (result.IsFailure) return HandleError(result.Error!);
        return Ok(ApiResponse<string>.Ok(MessageConstants.Get(MessageConstants.Updated)));
    }

    protected override IRequest<Result<PagedResult<AddressDto>>> CreatePagedQuery(int pageIndex, int pageSize, string? search, SortDTO? sort)
        => throw new NotImplementedException("User addresses are retrieved via get method without paging for now, using GetMyAddresses alias.");

    protected override IRequest<Result<AddressDto>> CreateGetByCodeQuery(string code)
        => new GetByCodeQuery<AddressDto>(code);

    protected override IRequest<Result<AddressDto>> CreateCreateCommand(CreateAddressDto dto)
        => new CreateCommand<CreateAddressDto, AddressDto>(dto);

    protected override IRequest<Result<AddressDto>> CreateUpdateCommand(string code, UpdateAddressDto dto)
        => new UpdateCommand<UpdateAddressDto, AddressDto>(code, dto);

    protected override IRequest<Result> CreateDeleteCommand(string code)
        => new DeleteCommand<TblAddress>(code);
}
