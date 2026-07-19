using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Carts.Handlers;

public class CartHandlers :
    IRequestHandler<GetMyCartQuery, Result<CartDto>>,
    IRequestHandler<GetCartQuery, Result<CartDto>>,
    IRequestHandler<AddToCartCommand, Result<CartDto>>,
    IRequestHandler<UpdateCartItemCommand, Result<CartDto>>,
    IRequestHandler<RemoveFromCartCommand, Result<CartDto>>,
    IRequestHandler<ClearCartCommand, Result<bool>>,
    IRequestHandler<AddMultipleToCartCommand, Result<CartDto>>
{
    private readonly ICartService _cartService;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CartHandlers> _logger;

    public CartHandlers(
        ICartService cartService,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CartHandlers> logger)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CartDto>> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, false, cancellationToken);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, false, cancellationToken);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var product = await _productRepository.AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == request.ProductCode, cancellationToken);

            if (product == null)
            {
                return Result.Failure<CartDto>(Error.NotFound(MessageConstants.Product, request.ProductCode));
            }

            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, true, cancellationToken);
            
            try
            {
                cart.AddItem(request.ProductCode, request.Quantity, request.Size, request.Color, product.StockQuantity ?? int.MaxValue);
                
                var addedOrUpdatedItem = cart.TblCartItems.FirstOrDefault(ci => 
                    ci.ProductCode == request.ProductCode && 
                    ci.Size == request.Size && 
                    ci.Color == request.Color);
                if (addedOrUpdatedItem != null)
                {
                    addedOrUpdatedItem.SetProduct(product);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<CartDto>(Error.Validation(MessageConstants.InsufficientStock, product.Name));
            }

            _logger.LogInformation("[AddToCart] Successfully processed product {ProductCode} for cart {CartCode}", 
                request.ProductCode, cart.Code);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, true, cancellationToken);
            
            var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.Code == request.CartItemCode);
            if (cartItem == null)
            {
                 return Result.Failure<CartDto>(Error.NotFound(MessageConstants.OrderItem, request.CartItemCode));
            }

            var product = await _productRepository.AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == cartItem.ProductCode, cancellationToken);
            int maxStock = product?.StockQuantity ?? int.MaxValue;

            try
            {
                cart.UpdateItem(request.CartItemCode, request.Quantity, maxStock);
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                 return Result.Failure<CartDto>(Error.Validation(MessageConstants.InsufficientStock, product?.Name ?? "Sản phẩm"));
            }
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, true, cancellationToken);
            
            cart.RemoveItem(request.CartItemCode);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, true, cancellationToken);
            cart.Clear();
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(true);
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(AddMultipleToCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(request.UserCode, async () =>
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Result.Failure<CartDto>(Error.Validation("ItemsRequired", "Danh sách sản phẩm không được để trống"));
            }

            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, true, cancellationToken);
            
            var productCodes = request.Items.Select(i => i.ProductCode).Distinct().ToList();
            var products = await _productRepository.AsQueryable()
                .AsNoTracking()
                .Where(p => productCodes.Contains(p.Code))
                .ToDictionaryAsync(p => p.Code, p => p, cancellationToken);

            foreach (var item in request.Items)
            {
                if (!products.TryGetValue(item.ProductCode, out var product)) continue;

                try
                {
                    cart.AddItem(item.ProductCode, item.Quantity, item.Size, item.Color, product.StockQuantity ?? int.MaxValue);
                    
                    var addedOrUpdatedItem = cart.TblCartItems.FirstOrDefault(ci => 
                        ci.ProductCode == item.ProductCode && 
                        ci.Size == item.Size && 
                        ci.Color == item.Color);
                    if (addedOrUpdatedItem != null)
                    {
                        addedOrUpdatedItem.SetProduct(product);
                    }
                }
                catch (InvalidOperationException) { }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    private async Task<T> ExecuteWithRetryAsync<T>(string lockKey, Func<Task<T>> action, CancellationToken cancellationToken, int maxRetries = 5)
    {
        int retryCount = 0;
        Random random = new Random();

        while (retryCount < maxRetries)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                // PostgreSQL Advisory Lock: serialized execution per user/key
                await _unitOfWork.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}))", cancellationToken, "cart_lock_" + lockKey);
                
                // IMPORTANT: Clear change tracker immediately after getting the lock
                // This ensures that any data loaded inside 'action' will be fresh from the database
                _unitOfWork.ClearChangeTracker();
                
                var result = await action();
                
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                
                if (retryCount > 0)
                {
                    _logger.LogInformation("[Cart] Concurrency conflict for {LockKey} resolved after {Attempt} retries.", lockKey, retryCount);
                }
                
                return result;
            }
            catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                retryCount++;
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "[Cart] Fatal concurrency error for {LockKey} after {MaxRetries} retries.", lockKey, maxRetries);
                    throw;
                }

                _logger.LogWarning("[Cart] Retrying {LockKey} due to conflict (Attempt {Attempt}/{MaxRetries})", 
                    lockKey, retryCount, maxRetries);

                _unitOfWork.ClearChangeTracker();
                
                int delay = (int)(100 * Math.Pow(2, retryCount)) + random.Next(1, 100);
                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        
        throw new Exception($"Failed to complete cart operation for {lockKey} after {maxRetries} retries.");
    }

}

