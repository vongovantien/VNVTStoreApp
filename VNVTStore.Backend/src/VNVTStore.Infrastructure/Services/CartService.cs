using Microsoft.EntityFrameworkCore;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence.Repositories;

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

    public async Task<TblCart> GetOrCreateCartAsync(string userCode, bool lockCart = false, CancellationToken cancellationToken = default)
    {
        TblCart? cart;

        if (lockCart && _cartRepository is Repository<TblCart> repo)
        {
            // PostgreSQL specific pessimistic lock. Requires an active transaction.
            cart = await repo.DbSet
                .FromSqlRaw("SELECT * FROM \"TblCart\" WHERE \"UserCode\" = {0} FOR UPDATE", userCode)
                .Include(c => c.TblCartItems)
                .ThenInclude(ci => ci.ProductCodeNavigation)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            cart = await _cartRepository.AsQueryable()
                .Include(c => c.TblCartItems)
                .ThenInclude(ci => ci.ProductCodeNavigation)
                .FirstOrDefaultAsync(c => c.UserCode == userCode, cancellationToken);
        }

        if (cart == null)
        {
            cart = TblCart.Create(userCode);
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            // Reload to get DB-generated values
            await _cartRepository.ReloadAsync(cart, cancellationToken);
        }

        return cart;
    }

    public async Task ClearCartAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userCode, true, cancellationToken);
        cart.TblCartItems.Clear();
        _cartRepository.Update(cart);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
