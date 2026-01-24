using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    IRequestHandler<ClearCartCommand, Result<bool>>
{
    private readonly ICartService _cartService;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartHandlers(
        ICartService cartService,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        var product = await _productRepository.GetByCodeAsync(request.ProductCode, cancellationToken);
        if (product == null)
        {
            return Result.Failure<CartDto>(Error.NotFound(MessageConstants.Product, request.ProductCode));
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);

            try
            {
                cart.AddItem(request.ProductCode, request.Quantity, request.Size, request.Color, product.StockQuantity ?? 0);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<CartDto>(Error.Validation("InsufficientStock", ex.Message));
            }

            await _unitOfWork.CommitAsync(cancellationToken);
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

            var product = await _productRepository.GetByCodeAsync(cartItem.ProductCode, cancellationToken);
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

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken, int maxRetries = 10)
    {
        int retryCount = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException)
            {
                retryCount++;
                if (retryCount >= maxRetries) throw;

                // Clear tracking to reload fresh entities in next attempt
                _unitOfWork.ClearChangeTracker();
                
                // Exponential backoff with jitter
                var delay = TimeSpan.FromMilliseconds(new Random().Next(100, 300) * retryCount);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
