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
    DbSet<TblOrder> TblOrders { get; set; }
    DbSet<TblAddress> TblAddresses { get; set; }
    DbSet<TblCart> TblCarts { get; set; }
    DbSet<TblCartItem> TblCartItems { get; set; }
    DbSet<TblCategory> TblCategories { get; set; }
    DbSet<TblCoupon> TblCoupons { get; set; }
    DbSet<TblOrderItem> TblOrderItems { get; set; }
    DbSet<TblDelivery> TblDeliveries { get; set; }
    DbSet<TblDeliveryHistory> TblDeliveryHistories { get; set; }
    DbSet<TblPayment> TblPayments { get; set; }
    DbSet<TblProductPromotion> TblProductPromotions { get; }
    DbSet<TblPromotion> TblPromotions { get; }
    DbSet<TblReview> TblReviews { get; }
    DbSet<TblNews> TblNews { get; }
    DbSet<TblBrand> TblBrands { get; }
    DbSet<TblUnit> TblUnits { get; }
    DbSet<TblProductUnit> TblProductUnits { get; }
    DbSet<TblProductDetail> TblProductDetails { get; }
    DbSet<TblProductVariant> TblProductVariants { get; }
    DbSet<TblRole> TblRoles { get; }
    DbSet<TblPermission> TblPermissions { get; }
    DbSet<TblRolePermission> TblRolePermissions { get; }
    DbSet<TblTag> TblTags { get; }
    DbSet<TblProductTag> TblProductTags { get; }
    DbSet<TblQuoteItem> TblQuoteItems { get; }
    DbSet<TblSupplier> TblSuppliers { get; }
    DbSet<TblDebtLog> TblDebtLogs { get; }
    DbSet<TblSystemConfig> TblSystemConfigs { get; }
    DbSet<TblSystemSecret> TblSystemSecrets { get; }
    DbSet<TblMenu> TblMenus { get; }
    DbSet<TblRoleMenu> TblRoleMenus { get; }
    DbSet<TblAuditLog> TblAuditLogs { get; }
    DbSet<TblNotification> TblNotifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
