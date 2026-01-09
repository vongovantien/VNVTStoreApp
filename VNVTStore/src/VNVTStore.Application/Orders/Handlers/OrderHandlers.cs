using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;

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
    private readonly ICartService _cartService;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblAddress> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderHandlers(
        IRepository<TblOrder> orderRepository,
        ICartService cartService,
        IRepository<TblProduct> productRepository,
        IRepository<TblAddress> addressRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Get Cart
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);

        if (cart == null || !cart.TblCartItems.Any())
        {
            return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty));
        }

        // 2. Validate Stock and Calculate Total
        decimal totalAmount = 0;
        var orderItems = new List<TblOrderItem>();

        foreach (var item in cart.TblCartItems)
        {
            if (item.ProductCodeNavigation.StockQuantity < item.Quantity)
                return Result.Failure<OrderDto>(Error.Validation(MessageConstants.InsufficientStock, item.ProductCodeNavigation.Name));

            totalAmount += item.ProductCodeNavigation.Price * item.Quantity;

            // Reduce Stock
            item.ProductCodeNavigation.StockQuantity -= item.Quantity;
            _productRepository.Update(item.ProductCodeNavigation);

            orderItems.Add(new TblOrderItem
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                PriceAtOrder = item.ProductCodeNavigation.Price,
                Size = item.Size,
                Color = item.Color
            });
        }

        // 3. Calculate Shipping Fee
        // Rule: Free shipping for orders >= 1,000,000, else 30,000
        decimal shippingFee = totalAmount >= 1000000 ? 0 : 30000;
        decimal finalAmount = totalAmount + shippingFee; // - Discount (handled if Coupon logic is added)

        // Create Address if needed
        string addressCode = request.Dto.AddressCode;
        if (string.IsNullOrEmpty(addressCode))
        {
             if (!string.IsNullOrEmpty(request.Dto.Address))
             {
                 // Append Receiver Info and Note to AddressLine (Hack due to schema limitation)
                 var receiverInfo = $"Receiver: {request.Dto.FullName}, Phone: {request.Dto.Phone}";
                 var addressParts = new List<string> 
                 { 
                     request.Dto.Address, 
                     request.Dto.Ward, 
                     request.Dto.District 
                 };
                 var baseAddress = string.Join(", ", addressParts.Where(s => !string.IsNullOrEmpty(s)));
                 var fullAddressLine = $"{baseAddress} | {receiverInfo}";
                 if (!string.IsNullOrEmpty(request.Dto.Note))
                 {
                     fullAddressLine += $" | Note: {request.Dto.Note}";
                 }

                 var newAddress = new TblAddress
                 {
                     Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                     UserCode = request.UserCode,
                     AddressLine = fullAddressLine.Length > 255 ? fullAddressLine.Substring(0, 255) : fullAddressLine,
                     City = request.Dto.City,
                     CreatedAt = DateTime.UtcNow,
                     IsDefault = false
                 };
                 await _addressRepository.AddAsync(newAddress, cancellationToken);
                 addressCode = newAddress.Code;
             }
             else
             {
                  return Result.Failure<OrderDto>(Error.Validation("Address is required"));
             }
        }

        // 4. Create Order
        var order = new TblOrder
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            UserCode = request.UserCode,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending.ToString(),
            TotalAmount = totalAmount,
            ShippingFee = shippingFee,
            FinalAmount = finalAmount,
            AddressCode = addressCode,
            CouponCode = request.Dto.CouponCode, // Store coupon code if provided
            TblOrderItems = orderItems
        };

        // 5. Create Payment Record
        var payment = new TblPayment
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 10),
            OrderCode = order.Code,
            Amount = finalAmount,
            Method = request.Dto.PaymentMethod,
            Status = "Pending", // Payment status
            PaymentDate = null // Not paid yet
        };
        order.TblPayment = payment; // Link payment to order via Navigation property if exists, or add to DbSet if separate repository

        await _orderRepository.AddAsync(order, cancellationToken);

        // 6. Clear Cart
        await _cartService.ClearCartAsync(request.UserCode, cancellationToken);

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
             return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.OrderCode));

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
            return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.OrderCode));

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
            return Result.Failure<bool>(Error.NotFound(MessageConstants.Order, request.OrderCode));

        if (order.UserCode != request.UserCode)
             return Result.Failure<bool>(Error.Forbidden("Cannot cancel another user's order"));

        if (order.Status != OrderStatus.Pending.ToString())
             return Result.Failure<bool>(Error.Validation(MessageConstants.OrderCannotCancel));

        // Restore stock
        foreach(var item in order.TblOrderItems)
        {
            item.ProductCodeNavigation.StockQuantity += item.Quantity;
             _productRepository.Update(item.ProductCodeNavigation);
        }

        order.Status = OrderStatus.Cancelled.ToString();
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
