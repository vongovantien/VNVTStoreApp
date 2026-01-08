using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNVTStore.Application.Suppliers.Commands;
using VNVTStore.Application.Suppliers.Queries;

namespace VNVTStore.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all suppliers (Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetAllSuppliersQuery(pageIndex, pageSize, search, isActive));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get supplier by code
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetSupplier(string code)
    {
        var result = await _mediator.Send(new GetSupplierByCodeQuery(code));
        if (result.IsFailure) return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create supplier
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var result = await _mediator.Send(new CreateSupplierCommand(
            request.Name, request.ContactPerson, request.Email, request.Phone,
            request.Address, request.TaxCode, request.BankAccount, request.BankName, request.Notes));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetSupplier), new { code = result.Value!.Code }, result.Value);
    }

    /// <summary>
    /// Update supplier
    /// </summary>
    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateSupplier(string code, [FromBody] UpdateSupplierRequest request)
    {
        var result = await _mediator.Send(new UpdateSupplierCommand(
            code, request.Name, request.ContactPerson, request.Email, request.Phone,
            request.Address, request.TaxCode, request.BankAccount, request.BankName, request.Notes, request.IsActive));
        
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete supplier
    /// </summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteSupplier(string code)
    {
        var result = await _mediator.Send(new DeleteSupplierCommand(code));
        if (result.IsFailure) return BadRequest(result.Error);
        return NoContent();
    }
}

public record CreateSupplierRequest(
    string Name, string? ContactPerson, string? Email, string? Phone,
    string? Address, string? TaxCode, string? BankAccount, string? BankName, string? Notes);

public record UpdateSupplierRequest(
    string? Name, string? ContactPerson, string? Email, string? Phone,
    string? Address, string? TaxCode, string? BankAccount, string? BankName, string? Notes, bool? IsActive);
