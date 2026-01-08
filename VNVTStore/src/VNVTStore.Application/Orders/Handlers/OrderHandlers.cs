using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Orders.Handlers;

public class OrderHandlers :
    IRequestHandler<CreateOrderCommand, Result<OrderDto>>,
    IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderDto>>>,
    IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>,
    IRequestHandler<GetAllOrdersQuery, Result<PagedResult<OrderDto>>>,
    IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>,
    IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly IRepository<TblCart> _cartRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderHandlers(
        IRepository<TblOrder> orderRepository,
        IRepository<TblCart> cartRepository,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Get Cart
        var cart = await _cartRepository.AsQueryable()
            .Include(c => c.TblCartItems)
            .ThenInclude(ci => ci.ProductCodeNavigation)
            .FirstOrDefaultAsync(c => c.UserCode == request.UserCode, cancellationToken);

        if (cart == null || !cart.TblCartItems.Any())
        {
            return Result.Failure<OrderDto>(Error.Validation("Order", "Cart is empty"));
        }

        // 2. Validate Stock and Calculate Total
        decimal totalAmount = 0;
        var orderItems = new List<TblOrderItem>();

        foreach (var item in cart.TblCartItems)
        {
            if (item.ProductCodeNavigation.StockQuantity < item.Quantity)
            {
                return Result.Failure<OrderDto>(Error.Validation("Order", $"Product {item.ProductCodeNavigation.Name} insufficient stock"));
            }

            totalAmount += item.ProductCodeNavigation.Price * item.Quantity;

            // Reduce Stock
            item.ProductCodeNavigation.StockQuantity -= item.Quantity;
            _productRepository.Update(item.ProductCodeNavigation);

            orderItems.Add(new TblOrderItem
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                PriceAtOrder = item.ProductCodeNavigation.Price
            });
        }

        // 3. Create Order
        var order = new TblOrder
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10), // Or use proper ID generation
            UserCode = request.UserCode,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = totalAmount,
            FinalAmount = totalAmount, // - Discount
            AddressCode = request.Dto.AddressCode,
            TblOrderItems = orderItems
        };

        // Note: Coupon logic can be added here

        await _orderRepository.AddAsync(order, cancellationToken);

        // 4. Clear Cart
        cart.TblCartItems.Clear();
        _cartRepository.Update(cart);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _orderRepository.AsQueryable()
            .Where(o => o.UserCode == request.UserCode);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(o => o.Status == request.Status);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(o => o.TblOrderItems)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<OrderDto>>(items);
        return Result.Success(new PagedResult<OrderDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.AsQueryable()
            .Include(o => o.TblOrderItems)
            .ThenInclude(oi => oi.ProductCodeNavigation)
            .FirstOrDefaultAsync(o => o.Code == request.OrderCode, cancellationToken);
            
        if (order == null)
             return Result.Failure<OrderDto>(Error.NotFound("Order", request.OrderCode));

        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _orderRepository.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
             query = query.Where(o => o.Status == request.Status);
        
        // Search by user code or order code logic if needed (skipped for brevity)

         var totalItems = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(o => o.OrderDate)
             .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(o => o.TblOrderItems)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<OrderDto>>(items);
        return Result.Success(new PagedResult<OrderDto>(dtos, totalItems, request.PageIndex, request.PageSize));
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.OrderCode, cancellationToken);
        if (order == null)
            return Result.Failure<OrderDto>(Error.NotFound("Order", request.OrderCode));

        order.Status = request.Status;
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);
        
        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
         var order = await _orderRepository.AsQueryable()
              .Include(o => o.TblOrderItems)
              .ThenInclude(oi => oi.ProductCodeNavigation)
             .FirstOrDefaultAsync(o => o.Code == request.OrderCode, cancellationToken);

        if (order == null)
            return Result.Failure<bool>(Error.NotFound("Order", request.OrderCode));

        if (order.UserCode != request.UserCode)
             return Result.Failure<bool>(Error.Forbidden("Cannot cancel another user's order"));

        if (order.Status != "Pending")
             return Result.Failure<bool>(Error.Validation("Order", "Can only cancel pending orders"));

        // Restore stock
        foreach(var item in order.TblOrderItems)
        {
            item.ProductCodeNavigation.StockQuantity += item.Quantity;
             _productRepository.Update(item.ProductCodeNavigation);
        }

        order.Status = "Cancelled";
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
