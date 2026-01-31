using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        IConfiguration configuration) : base(orderRepository, unitOfWork, mapper, dapperContext)
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
            
            if (!string.IsNullOrEmpty(request.userCode))
            {
                var cart = await _cartService.GetOrCreateCartAsync(userCode, cancellationToken);
                if (cart == null || !cart.TblCartItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken); 
                    return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty));
                }

                var productCodes = cart.TblCartItems.Select(c => c.ProductCode).Distinct().ToList();
                var files = await _context.TblFiles
                    .Where(f => productCodes.Contains(f.MasterCode) && f.MasterType == "Product")
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
                     return Result.Failure<OrderDto>(Error.Validation(MessageConstants.CartEmpty));
                }

                foreach(var dtoItem in request.dto.Items)
                {
                     if (string.IsNullOrEmpty(dtoItem.ProductCode)) continue;
                     
                     var product = await _productRepository.AsQueryable()
                         .FirstOrDefaultAsync(p => p.Code == dtoItem.ProductCode, cancellationToken);
                     if (product == null) continue;

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

            decimal shippingFee = _shippingStrategy.CalculateShippingFee(totalAmount);

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
                             null, 
                             null, 
                             null 
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

            var order = TblOrder.Create(
                userCode,
                addressCode,
                totalAmount,
                shippingFee,
                0, 
                request.dto.CouponCode
            );

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

            await _mediator.Publish(new OrderCreatedEvent(order, userCode, request.dto.CartCode), cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

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

    private record ProcessableOrderItem(string ProductCode, TblProduct ProductCodeNavigation, int Quantity, string? Size, string? Color, string? ImageURL);
}
