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
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var product = await _productRepository.AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == request.ProductCode, cancellationToken);

            if (product == null)
            {
                return Result.Failure<CartDto>(Error.NotFound(MessageConstants.Product, request.ProductCode));
            }

            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
            _logger.LogInformation("[AddToCart] Cart {CartCode} loaded for user {UserCode}, items count: {Count}", 
                cart.Code, request.UserCode, cart.TblCartItems.Count);

            try
            {
                cart.AddItem(request.ProductCode, request.Quantity, request.Size, request.Color, product.StockQuantity ?? 0);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<CartDto>(Error.Validation("InsufficientStock", ex.Message));
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("[AddToCart] Successfully added product {ProductCode} to cart {CartCode}", 
                request.ProductCode, cart.Code);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
            
            var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.Code == request.CartItemCode);
            if (cartItem == null)
            {
                 return Result.Failure<CartDto>(Error.NotFound(MessageConstants.OrderItem, request.CartItemCode));
            }

            var product = await _productRepository.AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == cartItem.ProductCode, cancellationToken);
            int maxStock = product?.StockQuantity ?? 0;

            try
            {
                cart.UpdateItem(request.CartItemCode, request.Quantity, maxStock);
            }
            catch (InvalidOperationException ex)
            {
                 return Result.Failure<CartDto>(Error.Validation("InsufficientStock", ex.Message));
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    public async Task<Result<CartDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        
        cart.RemoveItem(request.CartItemCode);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        cart.Clear();
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(true);
    }

    public async Task<Result<CartDto>> Handle(AddMultipleToCartCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Result.Failure<CartDto>(Error.Validation("ItemsRequired", "Danh sách sản phẩm không được để trống"));
            }

            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
            _logger.LogInformation("[AddMultipleToCart] Cart {CartCode} loaded for user {UserCode}, items count: {Count}", 
                cart.Code, request.UserCode, cart.TblCartItems.Count);

            var productCodes = request.Items.Select(i => i.ProductCode).Distinct().ToList();
            var products = await _productRepository.AsQueryable()
                .AsNoTracking()
                .Where(p => productCodes.Contains(p.Code))
                .ToDictionaryAsync(p => p.Code, p => p, cancellationToken);

            foreach (var item in request.Items)
            {
                if (!products.TryGetValue(item.ProductCode, out var product))
                {
                    _logger.LogWarning("[AddMultipleToCart] Product {ProductCode} not found, skipping", item.ProductCode);
                    continue;
                }

                try
                {
                    cart.AddItem(item.ProductCode, item.Quantity, item.Size, item.Color, product.StockQuantity ?? 0);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("[AddMultipleToCart] Insufficient stock for {ProductCode}: {Message}", item.ProductCode, ex.Message);
                    // For bulk add, we might want to continue adding other items even if one fails stock check
                    // or return failure for the whole batch. Usually better to add what's possible OR return list of errors.
                    // For now, let's keep it simple and skip failed items, but log them.
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("[AddMultipleToCart] Successfully added items to cart {CartCode}", cart.Code);
            return Result.Success(_mapper.Map<CartDto>(cart));
        }, cancellationToken);
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken, int maxRetries = 5)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                retryCount++;
                var entries = string.Join(", ", ex.Entries.Select(e => $"{e.Entity.GetType().Name}[{e.State}]"));
                _logger.LogWarning(ex, "[Cart] Concurrency conflict on attempt {Attempt}/{MaxRetries}. Entities: {Entries}", 
                    retryCount, maxRetries, entries);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "[Cart] Concurrency conflict persists after {MaxRetries} retries. Request failing.", maxRetries);
                    throw;
                }

                // Prepare for retry: Clear current context state to ensure next fetch gets DB values
                _unitOfWork.ClearChangeTracker();
                
                // Exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(50 * Math.Pow(2, retryCount)), cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Handle database exceptions that might be transitional (e.g., unique index violation during simultaneous adds)
                retryCount++;
                _logger.LogWarning(ex, "[Cart] Database update exception on attempt {Attempt}/{MaxRetries}: {Message}", 
                    retryCount, maxRetries, ex.InnerException?.Message ?? ex.Message);
                
                if (retryCount >= maxRetries) throw;

                _unitOfWork.ClearChangeTracker();
                await Task.Delay(TimeSpan.FromMilliseconds(50 * Math.Pow(2, retryCount)), cancellationToken);
            }
        }
    }
}

