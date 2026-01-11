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
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetOrCreateCartAsync(request.UserCode, cancellationToken);
        
        // Need to load product to check stock if increasing quantity. 
        // But UpdateItem logic in Domain checks against MaxStock passed in.
        // So we need to find the product associated with the cart item.
        // However, Domain method `UpdateItem` takes `maxStock`.
        // We first find the item to get ProductCode. Is that available? 
        // TblCartItems might be loaded.
        
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
}
