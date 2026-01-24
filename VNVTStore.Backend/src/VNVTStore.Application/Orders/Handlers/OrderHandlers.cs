using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Application.Orders.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Strategies;
using VNVTStore.Domain.Events;
using VNVTStore.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace VNVTStore.Application.Orders.Handlers;

public class OrderHandlers : BaseHandler<TblOrder>,
    IRequestHandler<CreateOrderCommand, Result<OrderDto>>,
    IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderDto>>>,
    IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>,
    IRequestHandler<GetAllOrdersQuery, Result<PagedResult<OrderDto>>>,
    IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>,
    IRequestHandler<CancelOrderCommand, Result<bool>>,
    IRequestHandler<VerifyOrderCommand, Result<string>>
{
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly ICartService _cartService;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblAddress> _addressRepository;
    private readonly IRepository<TblUser> _userRepository; 
    private readonly IShippingStrategy _shippingStrategy;
    private readonly IMediator _mediator; 
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context; // Required for TblFile logic in CreateOrder
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IDapperContext _dapperContext;

    public OrderHandlers(
        IRepository<TblOrder> orderRepository,
        ICartService cartService,
        IRepository<TblProduct> productRepository,
        IRepository<TblAddress> addressRepository,
        IRepository<TblUser> userRepository,
        IShippingStrategy shippingStrategy, 
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationService notificationService,
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        IDapperContext dapperContext) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _userRepository = userRepository;
        _shippingStrategy = shippingStrategy;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _dapperContext = dapperContext;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var searchFields = new List<SearchDTO>();
        if (request.filters != null)
        {
            foreach (var filter in request.filters)
            {
                 if (string.IsNullOrEmpty(filter.Value)) continue;
                 // Map specific SearchParams keys to DB columns if needed, or pass through
                 if (filter.Key == "code") searchFields.Add(new SearchDTO { SearchField = "Code", SearchValue = filter.Value, SearchCondition = SearchCondition.Contains });
                 // Add more custom filters mapping here if needed
            }
        }
        
        if (request.status.HasValue)
        {
             searchFields.Add(new SearchDTO { SearchField = "Status", SearchValue = ((int)request.status.Value).ToString(), SearchCondition = SearchCondition.Equal });
        }


        return await GetPagedDapperAsync<OrderDto>(
            request.pageIndex,
            request.pageSize,
            searchFields,
            null, // SortDTO default
            null, // ReferenceTables (auto-discovered)
            null, // Fields
            cancellationToken
        );
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            string userCode = request.userCode;
            if (string.IsNullOrEmpty(userCode))
            {
                userCode = await GetOrCreateGuestUser(cancellationToken);
            }

            // 1. Get Items to Process (Common Structure)
            List<ProcessableOrderItem> itemsToProcess = new();
            
            if (!string.IsNullOrEmpty(request.userCode))
            {
                var cart = await _cartService.GetOrCreateCartAsync(userCode, cancellationToken);
                if (cart == null || !cart.TblCartItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken); 
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty));
                }

                // Batch fetch images
                var productCodes = cart.TblCartItems.Select(c => c.ProductCode).Distinct().ToList();
                var files = await _context.TblFiles
                    .Where(f => productCodes.Contains(f.MasterCode) && f.MasterType == "Product")
                    .ToListAsync(cancellationToken);
                var fileMap = files.GroupBy(f => f.MasterCode).ToDictionary(g => g.Key, g => g.First().Path);

                itemsToProcess = cart.TblCartItems.Select(ci => new ProcessableOrderItem(
                    ci.ProductCode,
                    ci.ProductCodeNavigation,
                    ci.Quantity,
                    ci.Size,
                    ci.Color,
                    fileMap.ContainsKey(ci.ProductCode) ? fileMap[ci.ProductCode] : null
                )).ToList();
            }
            else
            {
                // Guest Checkout - items from DTO
                if (request.dto.Items == null || !request.dto.Items.Any())
                {
                     await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                     return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty));
                }

                itemsToProcess = new List<ProcessableOrderItem>();
                foreach(var dtoItem in request.dto.Items)
                {
                     if (string.IsNullOrEmpty(dtoItem.ProductCode)) continue;
                     
                     var product = await _productRepository.AsQueryable()
                         .FirstOrDefaultAsync(p => p.Code == dtoItem.ProductCode, cancellationToken);
                     if (product == null) continue;

                     // Fetch image
                     var file = await _context.TblFiles.FirstOrDefaultAsync(f => f.MasterCode == product.Code && f.MasterType == "Product", cancellationToken);

                     itemsToProcess.Add(new ProcessableOrderItem(
                         product.Code,
                         product,
                         dtoItem.Quantity,
                         dtoItem.Size,
                         dtoItem.Color,
                         file?.Path
                     ));
                }
            }


            // 2. Validate Stock and Calculate Total
            decimal totalAmount = 0;
            var orderItems = new List<TblOrderItem>();

            foreach (var item in itemsToProcess)
            {
                try
                {
                    // Reload stock from DB to ensure we have the latest value before deducting
                    await _productRepository.ReloadAsync(item.ProductCodeNavigation, cancellationToken);

                    // Verify logic inside Product - DeductStock
                    item.ProductCodeNavigation.DeductStock(item.Quantity);
                    _productRepository.Update(item.ProductCodeNavigation);
                }
                catch (InvalidOperationException ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.InsufficientStock, item.ProductCodeNavigation.Name));
                }

                totalAmount += item.ProductCodeNavigation.Price * item.Quantity;

                orderItems.Add(TblOrderItem.Create(
                    item.ProductCode!,
                    item.ProductCodeNavigation.Name,
                    item.ImageURL,
                    item.Quantity,
                    item.ProductCodeNavigation.Price,
                    item.Size,
                    item.Color
                ));
            }

            // 3. Calculate Shipping Fee via Strategy
            decimal shippingFee = _shippingStrategy.CalculateShippingFee(totalAmount);

            // Create Address if needed
            string addressCode = request.dto.AddressCode;
            if (string.IsNullOrEmpty(addressCode))
            {
                 if (!string.IsNullOrEmpty(request.dto.Address))
                 {
                     var receiverInfo = $"Receiver: {request.dto.FullName}, Phone: {request.dto.Phone}";
                     var addressParts = new List<string> { request.dto.Address, request.dto.Ward, request.dto.District };
                     var baseAddress = string.Join(", ", addressParts.Where(s => !string.IsNullOrEmpty(s)));
                     var fullAddressLine = $"{baseAddress} | {receiverInfo}";
                     if (!string.IsNullOrEmpty(request.dto.Note)) fullAddressLine += $" | Note: {request.dto.Note}";
    
                     var addressBuilder = new TblAddress.Builder()
                         .WithUser(userCode)
                         .AtLocation(
                             fullAddressLine.Length > 255 ? fullAddressLine.Substring(0, 255) : fullAddressLine,
                             request.dto.City,
                             null, // State
                             null, // PostalCode
                             null // Country
                         );
                     
                     var newAddress = addressBuilder.Build();
                     await _addressRepository.AddAsync(newAddress, cancellationToken);
                     addressCode = newAddress.Code;
                 }
                 else
                 {
                      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                      return Result.Failure<OrderDto>(Error.Validation("Address is required"));
                 }
            }

            // 4. Create Order using Factory
            var order = TblOrder.Create(
                userCode,
                addressCode,
                totalAmount,
                shippingFee,
                0, // Discount 
                request.dto.CouponCode
            );

            foreach (var oi in orderItems)
            {
                order.AddOrderItem(oi);
            }

            // 5. Save Order
            var token = Guid.NewGuid().ToString("N");
            order.SetVerificationToken(token, DateTime.UtcNow.AddMinutes(30));

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 6. Award Loyalty Points (Registered Users only)
            if (!string.IsNullOrEmpty(request.userCode) && userCode != "USR_GUEST") // Assuming logic for USR_GUEST
            {
                 var user = await _userRepository.GetByCodeAsync(userCode, cancellationToken);
                 if (user != null)
                 {
                      int points = (int)(totalAmount / 10000);
                      if (points > 0)
                      {
                           user.AddLoyaltyPoints(points);
                           _userRepository.Update(user);
                           await _unitOfWork.CommitAsync(cancellationToken);
                      }
                 }
            }

            // Publish Domain Event -> CartClearedEventHandler will pick this up
            await _mediator.Publish(new OrderCreatedEvent(order, userCode, request.dto.CartCode), cancellationToken);
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send Email Verification
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            var verifyLink = $"{frontendUrl}/verify-order?token={token}";
            var emailBody = $"<h3>Thank you for your order!</h3><p>Order Code: <b>{order.Code}</b></p><p>Please verify your order by clicking the link below:</p><a href='{verifyLink}'>Verify Order</a>";
            
            var userEmail = (await _userRepository.GetByCodeAsync(userCode, cancellationToken))?.Email;
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest@vnvtstore.com")
            {
                 userEmail = request.dto.Email;
            }
            
            if (!string.IsNullOrEmpty(userEmail))
            { 
                 try 
                 {
                    await _emailService.SendEmailAsync(userEmail, $"Verify your order {order.Code}", emailBody, true);
                 }
                 catch (Exception ex)
                 {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                 }
            }

            // Send Real-time Notification
            await _notificationService.BroadcastLocalizedAsync(MessageConstants.NotificationNewOrder, order.Code);

            return Result.Success(_mapper.Map<OrderDto>(order));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GetOrCreateGuestUser(CancellationToken cancellationToken)
    {
        var guestUser = await _userRepository.AsQueryable()
            .FirstOrDefaultAsync(u => u.Username == "guest", cancellationToken);

        if (guestUser == null)
        {
            guestUser = TblUser.Create(
                "guest",
                "guest@vnvtstore.com",
                "guest_pwd_hash_placeholder",
                "Khách vãng lai",
                UserRole.Customer
            );
            await _userRepository.AddAsync(guestUser, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken); 
        }
        return guestUser.Code;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        // Should also optimize GetMyOrdersQuery later, but user focused on Admin (GetAllOrders).
        // For consistency, let's leave this EF logic for now unless requested.
        var query = _orderRepository.AsQueryable()
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
        return Result.Success(new PagedResult<OrderDto>(dtos, totalItems));
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // ... (Keep EF for single get for now)
        var order = await _orderRepository.AsQueryable()
            .Include(o => o.TblOrderItems)
            .ThenInclude(oi => oi.ProductCodeNavigation)
            .FirstOrDefaultAsync(o => o.Code == request.orderCode, cancellationToken);
            
        if (order == null)
            return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByCodeAsync(request.orderCode, cancellationToken);
        if (order == null)
            return Result.Failure<OrderDto>(Error.NotFound(MessageConstants.Order, request.orderCode));

        order.UpdateStatus(request.status); // Use Domain Method
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);
        
        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
             var order = await _orderRepository.AsQueryable()
                  .Include(o => o.TblOrderItems)
                  .ThenInclude(oi => oi.ProductCodeNavigation)
                 .FirstOrDefaultAsync(o => o.Code == request.orderCode, cancellationToken);
    
            if (order == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<bool>(Error.NotFound(MessageConstants.Order, request.orderCode));
            }
    
            if (order.UserCode != request.userCode)
            {
                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                 return Result.Failure<bool>(Error.Forbidden("Cannot cancel another user's order"));
            }
    
            try 
            {
                order.Cancel(request.reason); // Use Domain Method
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<bool>(Error.Validation(ex.Message));
            }
    
            // Restore stock
            foreach(var item in order.TblOrderItems)
            {
                item.ProductCodeNavigation.RestoreStock(item.Quantity); // Use Domain Method
                 _productRepository.Update(item.ProductCodeNavigation);
            }
    
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
    
            return Result.Success(true);
        }
        catch (Exception)
        {
             await _unitOfWork.RollbackTransactionAsync(cancellationToken);
             throw;
        }
    }

    private record ProcessableOrderItem(string ProductCode, TblProduct ProductCodeNavigation, int Quantity, string? Size, string? Color, string? ImageURL);

    public async Task<Result<string>> Handle(VerifyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.TblOrders.FirstOrDefaultAsync(o => o.VerificationToken == request.Token, cancellationToken);
        
        if (order == null)
        {
            return Result.Failure<string>(Error.Validation("Invalid verification token."));
        }

        if (order.VerificationTokenExpiresAt < DateTime.UtcNow)
        {
             return Result.Failure<string>(Error.Validation("Verification token has expired."));
        }

        if (order.Status != OrderStatus.Pending)
        {
             return Result.Success("Order is already verified or processed.");
        }

        order.UpdateStatus(OrderStatus.Confirmed); // Or a specific Verified status if exists. Assuming Confirmed for now.
        // Clear token
        order.SetVerificationToken(null!, DateTime.MinValue); // Hacky clear
        
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(order.Code);
    }
}
