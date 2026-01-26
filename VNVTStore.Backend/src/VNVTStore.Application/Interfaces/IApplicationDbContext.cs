using Microsoft.EntityFrameworkCore;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TblUser> TblUsers { get; }
    DbSet<TblProduct> TblProducts { get; }
    DbSet<TblQuote> TblQuotes { get; }
    DbSet<TblBanner> TblBanners { get; }
    DbSet<TblFile> TblFiles { get; }
    DbSet<TblUserLogin> TblUserLogins { get; }
    DbSet<TblOrder> TblOrders { get; }
    DbSet<TblAddress> TblAddresses { get; }
    DbSet<TblCart> TblCarts { get; }
    DbSet<TblCartItem> TblCartItems { get; }
    DbSet<TblCategory> TblCategories { get; }
    DbSet<TblCoupon> TblCoupons { get; }
    DbSet<TblOrderItem> TblOrderItems { get; }
    DbSet<TblPayment> TblPayments { get; }
    DbSet<TblProductPromotion> TblProductPromotions { get; }
    DbSet<TblPromotion> TblPromotions { get; }
    DbSet<TblReview> TblReviews { get; }
    DbSet<TblBrand> TblBrands { get; }
    DbSet<TblUnit> TblUnits { get; }
    DbSet<TblProductUnit> TblProductUnits { get; }
    DbSet<TblProductDetail> TblProductDetails { get; }
    DbSet<TblTag> TblTags { get; }
    DbSet<TblProductTag> TblProductTags { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
