using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Carts.Commands;
using VNVTStore.Application.Carts.Queries;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Carts.Handlers;

public class CartHandlers :
    IRequestHandler<GetMyCartQuery, Result<CartDto>>,
    IRequestHandler<AddToCartCommand, Result<CartDto>>,
    IRequestHandler<UpdateCartItemCommand, Result<CartDto>>,
    IRequestHandler<RemoveFromCartCommand, Result<CartDto>>,
    IRequestHandler<ClearCartCommand, Result<bool>>
{
    private readonly IRepository<TblCart> _cartRepository;
    private readonly IRepository<TblProduct> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartHandlers(
        IRepository<TblCart> cartRepository,
        IRepository<TblProduct> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CartDto>> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(request.UserCode, cancellationToken);
        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByCodeAsync(request.ProductCode, cancellationToken);
        if (product == null)
        {
            return Result.Failure<CartDto>(Error.NotFound("Product", request.ProductCode));
        }

        if (product.StockQuantity < request.Quantity)
        {
            return Result.Failure<CartDto>(Error.Validation("Cart", "Insufficient stock"));
        }

        var cart = await GetOrCreateCartAsync(request.UserCode, cancellationToken);
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.ProductCode == request.ProductCode);

        if (cartItem == null)
        {
            cartItem = new TblCartItem
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                CartCode = cart.Code,
                ProductCode = request.ProductCode,
                Quantity = request.Quantity,
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
             return Result.Failure<CartDto>(Error.Validation("Cart", $"Insufficient stock. Available: {product.StockQuantity}"));
        }

        _cartRepository.Update(cart);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Map for response needs Product loaded
        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(request.UserCode, cancellationToken);
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.ProductCode == request.ProductCode);

        if (cartItem == null)
        {
            return Result.Failure<CartDto>(Error.NotFound("CartItem", request.ProductCode));
        }

        if (request.Quantity <= 0)
        {
            cart.TblCartItems.Remove(cartItem);
        }
        else
        {
            var product = await _productRepository.GetByCodeAsync(request.ProductCode, cancellationToken);
            if (product != null && product.StockQuantity < request.Quantity)
                  return Result.Failure<CartDto>(Error.Validation("Cart", $"Insufficient stock. Available: {product.StockQuantity}"));
            
            cartItem.Quantity = request.Quantity;
        }

        _cartRepository.Update(cart);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<CartDto>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(request.UserCode, cancellationToken);
        var cartItem = cart.TblCartItems.FirstOrDefault(ci => ci.ProductCode == request.ProductCode);

        if (cartItem != null)
        {
            cart.TblCartItems.Remove(cartItem);
            _cartRepository.Update(cart);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success(_mapper.Map<CartDto>(cart));
    }

    public async Task<Result<bool>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(request.UserCode, cancellationToken);
        cart.TblCartItems.Clear();
        _cartRepository.Update(cart);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }

    private async Task<TblCart> GetOrCreateCartAsync(string userCode, CancellationToken cancellationToken)
    {
        // Must include Product to calculate price?
        // Basic include needed for cartItems
         var cart = await _cartRepository.AsQueryable()
            .Include(c => c.TblCartItems)
            .ThenInclude(ci => ci.ProductCodeNavigation)
            .FirstOrDefaultAsync(c => c.UserCode == userCode, cancellationToken);

        if (cart == null)
        {
            cart = new TblCart
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                UserCode = userCode,
                CreatedAt = DateTime.UtcNow
            };
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return cart;
    }
}
