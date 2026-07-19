using AutoMapper;
using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Delivery.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using System.Linq;

namespace VNVTStore.Application.Delivery.Handlers;

public class GetDeliveryByOrderHandler : BaseHandler<TblDelivery>, IRequestHandler<GetDeliveryByOrderQuery, Result<DeliveryDto>>
{
    public GetDeliveryByOrderHandler(
        IRepository<TblDelivery> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext) : base(repository, unitOfWork, mapper, dapperContext)
    {
    }

    public async Task<Result<DeliveryDto>> Handle(GetDeliveryByOrderQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _repository.FindAsync(d => d.OrderCode == request.OrderCode, cancellationToken);
        
        if (delivery == null)
        {
            return Result.Failure<DeliveryDto>(Error.NotFound("Delivery", $"No delivery found for order {request.OrderCode}"));
        }

        var resultDto = _mapper.Map<DeliveryDto>(delivery);
        return Result.Success(resultDto);
    }
}

public class GetDeliveryHistoryHandler : IRequestHandler<GetDeliveryHistoryQuery, Result<List<DeliveryHistoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryHistoryHandler(
        IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<DeliveryHistoryDto>>> Handle(GetDeliveryHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = _context.TblDeliveryHistories.Where(h => h.DeliveryCode == request.DeliveryCode).ToList();
        
        var resultDto = _mapper.Map<List<DeliveryHistoryDto>>(history.OrderByDescending(h => h.Timestamp).ToList());
        return Result.Success(resultDto);
    }
}
