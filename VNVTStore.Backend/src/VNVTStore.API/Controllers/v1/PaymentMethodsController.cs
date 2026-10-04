using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using AutoMapper;

namespace VNVTStore.API.Controllers.v1;

public class PaymentMethodsController : BaseApiController<TblPaymentMethod, PaymentMethodDto, CreatePaymentMethodDto, UpdatePaymentMethodDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PaymentMethodsController(IMediator mediator, IApplicationDbContext context, IMapper mapper) : base(mediator)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get active payment methods for checkout (public)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePaymentMethods()
    {
        var methods = await _context.TblPaymentMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<PaymentMethodDto>>.Ok(_mapper.Map<IEnumerable<PaymentMethodDto>>(methods)));
    }
}
