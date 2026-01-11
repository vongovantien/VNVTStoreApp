using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IRepository<TblCart> _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IRepository<TblCart> cartRepository, IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TblCart> GetOrCreateCartAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.AsQueryable()
            .Include(c => c.TblCartItems)
            .ThenInclude(ci => ci.ProductCodeNavigation)
            .FirstOrDefaultAsync(c => c.UserCode == userCode, cancellationToken);

        if (cart == null)
        {
            cart = TblCart.Create(userCode);
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return cart;
    }

    public async Task ClearCartAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userCode, cancellationToken);
        cart.TblCartItems.Clear();
        _cartRepository.Update(cart);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
