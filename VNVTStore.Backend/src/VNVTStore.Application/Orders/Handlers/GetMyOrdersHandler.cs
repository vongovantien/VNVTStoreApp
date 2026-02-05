using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class GetMyOrdersHandler : BaseHandler<TblOrder>, 
    IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderDto>>>
{
    public GetMyOrdersHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.AsQueryable()
            .Where(o => o.UserCode == request.userCode);

        if (request.status.HasValue)
        {
            query = query.Where(o => o.Status == request.status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.pageIndex - 1) * request.pageSize)
            .Take(request.pageSize)
            .Include(o => o.TblOrderItems)
            .ThenInclude(oi => oi.ProductCodeNavigation)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<OrderDto>>(items);
        return Result.Success(new PagedResult<OrderDto>(dtos, totalItems, request.pageIndex, request.pageSize));
    }
}
