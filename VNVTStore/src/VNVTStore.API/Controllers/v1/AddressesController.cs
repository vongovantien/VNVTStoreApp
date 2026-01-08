using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Addresses.Commands;
using VNVTStore.Application.Addresses.Queries;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public AddressesController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private string GetUserCode() => _currentUser.UserCode ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Get all addresses for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await _mediator.Send(new GetUserAddressesQuery(GetUserCode()));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new address
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var result = await _mediator.Send(new CreateAddressCommand(
            GetUserCode(), request.AddressLine, request.City, request.State, 
            request.PostalCode, request.Country, request.IsDefault));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetMyAddresses), result.Value);
    }

    /// <summary>
    /// Update an address
    /// </summary>
    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateAddress(string code, [FromBody] UpdateAddressRequest request)
    {
        var result = await _mediator.Send(new UpdateAddressCommand(
            code, GetUserCode(), request.AddressLine, request.City, request.State, 
            request.PostalCode, request.IsDefault));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteAddress(string code)
    {
        var result = await _mediator.Send(new DeleteAddressCommand(code, GetUserCode()));
        if (result.IsFailure) return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Set address as default
    /// </summary>
    [HttpPost("{code}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(string code)
    {
        var result = await _mediator.Send(new SetDefaultAddressCommand(code, GetUserCode()));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(new { message = "Address set as default" });
    }
}

public record CreateAddressRequest(
    string AddressLine, string? City, string? State, 
    string? PostalCode, string? Country, bool IsDefault = false);

public record UpdateAddressRequest(
    string? AddressLine, string? City, string? State, 
    string? PostalCode, bool? IsDefault);
