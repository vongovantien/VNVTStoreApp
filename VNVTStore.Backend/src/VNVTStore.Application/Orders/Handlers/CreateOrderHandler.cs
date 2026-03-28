using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Common;
using VNVTStore.Application.Constants;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Orders.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Events;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Strategies;

namespace VNVTStore.Application.Orders.Handlers;

public class CreateOrderHandler : BaseHandler<TblOrder>,
    IRequestHandler<DeleteMultipleCommand<TblOrder>, Result>,
    IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly ICartService _cartService;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IRepository<TblAddress> _addressRepository;
    private readonly IRepository<TblUser> _userRepository; 
    private readonly IShippingStrategy _shippingStrategy;
    private readonly IMediator _mediator;
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILoyaltyService _loyaltyService;
    private readonly ISecretConfigurationService _secretConfig;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IRepository<TblOrder> orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDapperContext dapperContext,
        ICartService cartService,
        IRepository<TblProduct> productRepository,
        IRepository<TblAddress> addressRepository,
        IRepository<TblUser> userRepository,
        IShippingStrategy shippingStrategy, 
        IMediator mediator,
        INotificationService notificationService,
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILoyaltyService loyaltyService,
        ISecretConfigurationService secretConfig,
        ILogger<CreateOrderHandler> logger) : base(orderRepository, unitOfWork, mapper, dapperContext)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _userRepository = userRepository;
        _shippingStrategy = shippingStrategy;
        _mediator = mediator;
        _notificationService = notificationService;
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _loyaltyService = loyaltyService;
        _secretConfig = secretConfig;
        _logger = logger;
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

            List<ProcessableOrderItem> itemsToProcess = new();
            
            if (!string.IsNullOrEmpty(userCode))
            {
                // Console.WriteLine($"[HANDLER] Processing userCode: '{userCode}'");
                var cart = await _cartService.GetOrCreateCartAsync(userCode, cancellationToken);
                // Console.WriteLine($"[HANDLER] Cart: {(cart == null ? "null" : cart.TblCartItems.Count.ToString())} items");

                if (cart == null || !cart.TblCartItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken); 
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty, Array.Empty<object>()));
                }

                var productCodes = cart.TblCartItems.Select(c => c.ProductCode).Distinct().ToList();
                var files = await _context.TblFiles
                    .Where(f => f.MasterCode != null && productCodes.Contains(f.MasterCode) && f.MasterType == AppConstants.MasterTypes.Product)
                    .ToListAsync(cancellationToken);
                var fileMap = files
                    .GroupBy(f => f.MasterCode)
                    .Where(g => g.Key != null)
                    .ToDictionary(g => g.Key!, g => g.First().Path);

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
                if (request.dto.Items == null || !request.dto.Items.Any())
                {
                     await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                     return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty, Array.Empty<object>()));
                }

                // Batch load products to avoid N+1 query
                var productCodes = request.dto.Items
                    .Where(i => !string.IsNullOrEmpty(i.ProductCode))
                    .Select(i => i.ProductCode!)
                    .Distinct()
                    .ToList();

                var products = await _productRepository.AsQueryable()
                    .Where(p => productCodes.Contains(p.Code))
                    .ToDictionaryAsync(p => p.Code, cancellationToken);

                // Batch load files for all products
                var fileMap = await _context.TblFiles
                    .Where(f => f.MasterCode != null && productCodes.Contains(f.MasterCode) && f.MasterType == AppConstants.MasterTypes.Product)
                    .GroupBy(f => f.MasterCode)
                    .Select(g => new { Code = g.Key, Path = g.First().Path })
                    .ToDictionaryAsync(x => x.Code!, x => x.Path, cancellationToken);

                foreach (var dtoItem in request.dto.Items)
                {
                     if (string.IsNullOrEmpty(dtoItem.ProductCode)) continue;
                     if (!products.TryGetValue(dtoItem.ProductCode, out var product)) continue;

                     itemsToProcess.Add(new ProcessableOrderItem(
                         product.Code,
                         product,
                         dtoItem.Quantity,
                         dtoItem.Size,
                         dtoItem.Color,
                         fileMap.GetValueOrDefault(product.Code)
                     ));
                }
            }

            decimal totalAmount = 0;
            var orderItems = new List<TblOrderItem>();

            foreach (var item in itemsToProcess)
            {
                try
                {
                    await _productRepository.ReloadAsync(item.ProductCodeNavigation, cancellationToken);
                    item.ProductCodeNavigation.DeductStock(item.Quantity);
                    _productRepository.Update(item.ProductCodeNavigation);
                }
                catch (InvalidOperationException)
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


            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(request.dto.CouponCode))
            {
                // 1. Tìm trong TblCoupon trước
                var coupon = await _context.TblCoupons
                    .Include(c => c.PromotionCodeNavigation)
                    .FirstOrDefaultAsync(c => c.Code == request.dto.CouponCode, cancellationToken);

                TblPromotion? promo = null;

                if (coupon != null)
                {
                    // Coupon found → Validate coupon + linked promotion
                    if (!coupon.IsActive || coupon.PromotionCodeNavigation == null || !coupon.PromotionCodeNavigation.IsActive)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponNotActive, Array.Empty<object>()));
                    }

                    promo = coupon.PromotionCodeNavigation;

                    if (promo.UsageLimit.HasValue && (coupon.UsageCount ?? 0) >= promo.UsageLimit.Value)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponLimitReached, Array.Empty<object>()));
                    }
                }
                else
                {
                    // 2. Coupon not found → Fallback: Tìm trực tiếp trong TblPromotion
                    promo = await _context.TblPromotions
                        .FirstOrDefaultAsync(p => p.Code == request.dto.CouponCode, cancellationToken);

                    if (promo == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponNotFound, Array.Empty<object>()));
                    }

                    if (!promo.IsActive)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponNotActive, Array.Empty<object>()));
                    }
                }

                // Validate date range
                if (DateTime.UtcNow < promo.StartDate || DateTime.UtcNow > promo.EndDate)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponExpired, Array.Empty<object>()));
                }

                // Validate min order amount
                if (promo.MinOrderAmount.HasValue && totalAmount < promo.MinOrderAmount.Value)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CouponMinOrderAmountNotMet, Array.Empty<object>()));
                }

                // Discount calculation
                if (promo.DiscountType == "Percentage" || promo.DiscountType == "PERCENTAGE")
                {
                    discountAmount = (totalAmount * promo.DiscountValue) / 100;
                    if (promo.MaxDiscountAmount.HasValue && discountAmount > promo.MaxDiscountAmount.Value)
                        discountAmount = promo.MaxDiscountAmount.Value;
                }
                else // Fixed / AMOUNT
                {
                    discountAmount = promo.DiscountValue;
                }

                if (discountAmount > totalAmount) discountAmount = totalAmount;

                // Update usage count
                if (coupon != null)
                {
                    coupon.UsageCount = (coupon.UsageCount ?? 0) + 1;
                    _context.TblCoupons.Update(coupon);
                }
            }

            decimal shippingFee = _shippingStrategy.CalculateShippingFee(totalAmount - discountAmount);

            TblAddress? orderAddress = null;
            string addressCode = request.dto.AddressCode;
            if (string.IsNullOrEmpty(addressCode))
            {
                 if (!string.IsNullOrEmpty(request.dto.Address))
                 {
                     var receiverInfo = $"Receiver: {request.dto.FullName}, Phone: {request.dto.Phone}";
                     var addressParts = new List<string?> { request.dto.Address, request.dto.Ward, request.dto.District }.Where(x => !string.IsNullOrEmpty(x)).Cast<string>().ToList();
                     var baseAddress = string.Join(", ", addressParts.Where(s => !string.IsNullOrEmpty(s)));
                     var fullAddressLine = $"{baseAddress} | {receiverInfo}";
                     if (!string.IsNullOrEmpty(request.dto.Note)) fullAddressLine += $" | Note: {request.dto.Note}";
    
                     var addressBuilder = new TblAddress.Builder()
                         .WithUser(userCode)
                         .AtLocation(
                             fullAddressLine.Length > 255 ? fullAddressLine.Substring(0, 255) : fullAddressLine,
                             request.dto.City,
                             null, 
                             null, 
                             null 
                         );
                     
                     var newAddress = addressBuilder.Build();
                     await _addressRepository.AddAsync(newAddress, cancellationToken);
                     addressCode = newAddress.Code;
                     orderAddress = newAddress;
                 }
                 else
                 {
                      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                      return Result.Failure<OrderDto>(Error.Validation("Address is required"));
                 }
            }
            else
            {
                orderAddress = await _addressRepository.GetByCodeAsync(addressCode, cancellationToken);
            }

            var order = TblOrder.Create(
                userCode,
                addressCode,
                totalAmount,
                shippingFee,
                discountAmount, 
                request.dto.CouponCode
            );
            
            // Assign navigation property for DTO mapping
            if (orderAddress != null)
            {
                 order.AddressCodeNavigation = orderAddress;
            }

            foreach (var oi in orderItems)
            {
                order.AddOrderItem(oi);
            }

            var token = Guid.NewGuid().ToString("N");
            order.SetVerificationToken(token, DateTime.UtcNow.AddMinutes(30));

            await _repository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.userCode) && userCode != "USR_GUEST") 
            {
                 int points = await _loyaltyService.CalculatePointsForOrderAsync(totalAmount - discountAmount);
                 if (points > 0)
                 {
                      await _loyaltyService.AddPointsToUserAsync(userCode, points);
                 }
            }

            await _mediator.Publish(new OrderCreatedEvent(order, userCode, request.dto.CartCode), cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var frontendUrl = await _secretConfig.GetSecretAsync("FRONTEND_URL") ?? _configuration["FrontendUrl"] ?? "http://localhost:5173";
            var verifyLink = $"{frontendUrl}/verify-order?token={token}";
            
            var userEmail = (await _userRepository.GetByCodeAsync(userCode, cancellationToken))?.Email;
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest@vnvtstore.com")
            {
                 userEmail = request.dto.Email;
            }
            
            if (!string.IsNullOrEmpty(userEmail))
            { 
                 try 
                 {
                    var emailBody = $"<h3>Thank you for your order!</h3><p>Order Code: <b>{order.Code}</b></p><p>Please verify your order by clicking the link below:</p><a href='{verifyLink}'>Verify Order</a>";
                    await _emailService.SendEmailAsync(userEmail, $"Order Confirmation - {order.Code}", emailBody, true);
                 }
                 catch (Exception ex)
                 {
                    _logger.LogError(ex, "[Handle] error: Failed to send email to {Email}", userEmail);
                 }
            }

            // Send admin notification
            var adminEmail = await _secretConfig.GetSecretAsync("ADMIN_EMAIL") ?? _configuration["EmailSettings:AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail))
            {
                try
                {
                    var adminEmailBody = $"A new order has been placed. <br/> Order Code: <b>{order.Code}</b> <br/> Total: <b>{order.FinalAmount:N0} VND</b>";
                    await _emailService.SendEmailAsync(adminEmail, $"New Order Received - {order.Code}", adminEmailBody, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send admin notification email to {AdminEmail}", adminEmail);
                }
            }

            await _notificationService.BroadcastLocalizedAsync(MessageConstants.NotificationNewOrder, order.Code);

            if (!string.IsNullOrEmpty(userCode) && userCode != "USR_GUEST")
            {
                 await _notificationService.SendToUserAsync(userCode, 
                     "Order Placed", 
                     $"Your order #{order.Code} has been placed successfully.", 
                     "SUCCESS", 
                     $"/account/orders/{order.Code}");
            }

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

    private record ProcessableOrderItem(string ProductCode, TblProduct ProductCodeNavigation, int Quantity, string? Size, string? Color, string? ImageURL);
}
