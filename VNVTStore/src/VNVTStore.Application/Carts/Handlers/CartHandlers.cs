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

        if (product.StockQuantity < request.Quantity)
        {
            return Result.Failure<CartDto>(Error.Validation(MessageConstants.InsufficientStock, product.Name));
        }

        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        // Find item by Product AND Attributes
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => 
            ci.ProductCode == request.ProductCode && 
            ci.Size == request.Size && 
            ci.Color == request.Color);

        if (cartItem == null)
        {
            cartItem = new TblCartItem
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                CartCode = cart.Code,
                ProductCode = request.ProductCode,
                Quantity = request.Quantity,
                Size = request.Size,
                Color = request.Color,
                AddedAt = DateTime.UtcNow
            };
            cart.TblCartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += request.Quantity;
        }

        // Validate total quantity against stock
        if (cartItem.Quantity > product.StockQuantity)
        {
             return Result.Failure<CartDto>(Error.Validation(MessageConstants.InsufficientStock, product.Name));
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        // Map for response needs Product loaded
        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        // Find by CartItemCode instead of ProductCode
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.Code == request.CartItemCode);

        if (cartItem == null)
        {
            return Result.Failure<CartDto>(Error.NotFound(MessageConstants.OrderItem, request.CartItemCode));
        }

        if (request.Quantity <= 0)
        {
            cart.TblCartItems.Remove(cartItem);
        }
        else
        {
            var product = await _productRepository.GetByCodeAsync(cartItem.ProductCode, cancellationToken);
            if (product != null && product.StockQuantity < request.Quantity)
                  return Result.Failure<CartDto>(Error.Validation(MessageConstants.InsufficientStock, product.Name));
            
            cartItem.Quantity = request.Quantity;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.Code == request.CartItemCode);

        if (cartItem != null)
        {
            cart.TblCartItems.Remove(cartItem);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await _cartService.ClearCartAsync(request.UserCode, cancellationToken);
        return Result.Success(true);
    }
}
