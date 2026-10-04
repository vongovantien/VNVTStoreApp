using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common;
using VNVTStore.Application.Delivery.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Application.Delivery.Handlers;

public class GetDeliveriesHandler : IRequestHandler<GetDeliveriesQuery, Result<PagedResult<DeliveryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetDeliveriesHandler> _logger;

    public GetDeliveriesHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetDeliveriesHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<DeliveryDto>>> Handle(GetDeliveriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.TblDeliveries
                .Include(d => d.OrderCodeNavigation)
                    .ThenInclude(o => o.AddressCodeNavigation)
                .Include(d => d.OrderCodeNavigation)
                    .ThenInclude(o => o.UserCodeNavigation)
                .Include(d => d.TblDeliveryHistories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.ShipperCode))
            {
                query = query.Where(d => d.ShipperCode == request.ShipperCode);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DeliveryStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(d => d.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim().ToLower();
                query = query.Where(d =>
                    d.OrderCode.ToLower().Contains(s) ||
                    (d.TrackingNumber != null && d.TrackingNumber.ToLower().Contains(s)) ||
                    (d.ShipperName != null && d.ShipperName.ToLower().Contains(s)) ||
                    (d.CarrierName != null && d.CarrierName.ToLower().Contains(s)) ||
                    (d.OrderCodeNavigation.AddressCodeNavigation != null && d.OrderCodeNavigation.AddressCodeNavigation.FullName != null && d.OrderCodeNavigation.AddressCodeNavigation.FullName.ToLower().Contains(s)) ||
                    (d.OrderCodeNavigation.AddressCodeNavigation != null && d.OrderCodeNavigation.AddressCodeNavigation.Phone != null && d.OrderCodeNavigation.AddressCodeNavigation.Phone.ToLower().Contains(s))
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageIndex = Math.Max(1, request.PageIndex);
            var pageSize = Math.Max(1, Math.Min(100, request.PageSize));

            var deliveries = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = deliveries.Select(d =>
            {
                var dto = _mapper.Map<DeliveryDto>(d);
                var order = d.OrderCodeNavigation;
                if (order != null)
                {
                    var addr = order.AddressCodeNavigation;
                    var user = order.UserCodeNavigation;
                    dto.CustomerName = addr?.FullName ?? user?.FullName ?? "Khách hàng";
                    dto.CustomerPhone = addr?.Phone ?? "";
                    if (addr != null)
                    {
                        var parts = new[] { addr.AddressLine, addr.City, addr.State, addr.Country }
                            .Where(p => !string.IsNullOrWhiteSpace(p));
                        dto.DeliveryAddress = string.Join(", ", parts);
                    }
                    dto.OrderFinalAmount = order.FinalAmount;
                    dto.OrderStatus = order.Status.ToString();
                }
                return dto;
            }).ToList();

            var pagedResult = new PagedResult<DeliveryDto>(dtos, totalCount, pageIndex, pageSize);

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deliveries list");
            return Result.Failure<PagedResult<DeliveryDto>>(Error.Validation("Failed to retrieve deliveries"));
        }
    }
}
