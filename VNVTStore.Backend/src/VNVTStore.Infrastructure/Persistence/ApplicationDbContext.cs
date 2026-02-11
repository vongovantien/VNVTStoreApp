using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext, IApplicationDbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAddress> TblAddresses { get; set; }

    public virtual DbSet<TblBanner> TblBanners { get; set; }
    public virtual DbSet<TblSystemConfig> TblSystemConfigs { get; set; }

    public virtual DbSet<TblCart> TblCarts { get; set; }

    public virtual DbSet<TblCartItem> TblCartItems { get; set; }

    public virtual DbSet<TblCategory> TblCategories { get; set; }

    public virtual DbSet<TblCoupon> TblCoupons { get; set; }

    public virtual DbSet<TblOrder> TblOrders { get; set; }

    public virtual DbSet<TblOrderItem> TblOrderItems { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }



    public virtual DbSet<TblProductPromotion> TblProductPromotions { get; set; }

    public virtual DbSet<TblPromotion> TblPromotions { get; set; }

    public virtual DbSet<TblReview> TblReviews { get; set; }

    public virtual DbSet<TblNews> TblNews { get; set; }

    public virtual DbSet<TblQuote> TblQuotes { get; set; }

    public virtual DbSet<TblFile> TblFiles { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblBrand> TblBrands { get; set; }
    public virtual DbSet<TblUnit> TblUnits { get; set; }
    public virtual DbSet<TblProductUnit> TblProductUnits { get; set; }
    public virtual DbSet<TblProductDetail> TblProductDetails { get; set; }
    public virtual DbSet<TblProductVariant> TblProductVariants { get; set; }
    public virtual DbSet<TblRole> TblRoles { get; set; }
    public virtual DbSet<TblPermission> TblPermissions { get; set; }
    public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }
    public virtual DbSet<TblTag> TblTags { get; set; }
    public virtual DbSet<TblUserLogin> TblUserLogins { get; set; }
    public virtual DbSet<TblProductTag> TblProductTags { get; set; }
    public virtual DbSet<TblQuoteItem> TblQuoteItems { get; set; }
    public virtual DbSet<TblDebtLog> TblDebtLogs { get; set; }
    public virtual DbSet<TblSupplier> TblSuppliers { get; set; }
    public virtual DbSet<TblMenu> TblMenus { get; set; }
    public virtual DbSet<TblRoleMenu> TblRoleMenus { get; set; }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveColumnType("timestamp with time zone")
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HaveColumnType("timestamp with time zone")
            .HaveConversion<UtcNullableDateTimeConverter>();

        configurationBuilder.Properties<OrderStatus>().HaveConversion<EnumLowerCaseConverter<OrderStatus>>();
        configurationBuilder.Properties<UserRole>().HaveConversion<EnumLowerCaseConverter<UserRole>>();
        configurationBuilder.Properties<PaymentStatus>().HaveConversion<EnumLowerCaseConverter<PaymentStatus>>();
        configurationBuilder.Properties<PaymentMethod>().HaveConversion<EnumLowerCaseConverter<PaymentMethod>>();
        
        // Custom conversion for ProductDetailType to use Description attribute or just ToString. 
        // Let's use string conversion for now, uppercase.
        configurationBuilder.Properties<ProductDetailType>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBanner>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblBanner_pkey");

            entity.ToTable("TblBanner");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.LinkUrl).HasMaxLength(200);
            entity.Property(e => e.LinkText).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(0);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
        });

        modelBuilder.Entity<TblAddress>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblAddress_pkey");

            entity.ToTable("TblAddress");
            
            entity.Property(e => e.Category).HasColumnName("Category").HasMaxLength(50);

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code).HasMaxLength(100)
                    .HasDefaultValueSql("('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.AddressLine).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(50);
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("'Vietnam'::character varying");
            }
            else
            {
                entity.Property(e => e.Country).HasMaxLength(50).HasDefaultValue("Vietnam");
            }
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PostalCode).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.UserCode).HasMaxLength(100);

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblAddress_UserCode_fkey");

            entity.HasIndex(e => e.UserCode, "idx_address_user");
        });

        modelBuilder.Entity<TblCart>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCart_pkey");

            entity.ToTable("TblCart");

            entity.HasIndex(e => e.UserCode, "idx_cart_user");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UserCode).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblCarts)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblCart_UserCode_fkey");
        });

        modelBuilder.Entity<TblCartItem>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCartItem_pkey");

            entity.ToTable("TblCartItem");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.CartCode).HasMaxLength(100);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasOne(d => d.CartCodeNavigation).WithMany(p => p.TblCartItems)
                .HasForeignKey(d => d.CartCode)
                .HasConstraintName("TblCartItem_CartCode_fkey");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.TblCartItems)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblCartItem_ProductCode_fkey");
        });

        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCategory_pkey");

            entity.ToTable("TblCategory");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.ImageURL)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ParentCode).HasMaxLength(100);

            entity.HasOne(d => d.ParentCodeNavigation).WithMany(p => p.InverseParentCodeNavigation)
                .HasForeignKey(d => d.ParentCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblCategory_ParentCode_fkey");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            
            entity.HasIndex(e => e.ParentCode, "idx_category_parent");
            entity.HasIndex(e => e.Name, "idx_category_name");
        });

        modelBuilder.Entity<TblCoupon>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCoupon_pkey");

            entity.ToTable("TblCoupon");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.PromotionCode).HasMaxLength(100);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);

            entity.HasOne(d => d.PromotionCodeNavigation).WithMany(p => p.TblCoupons)
                .HasForeignKey(d => d.PromotionCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblCoupon_PromotionCode_fkey");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<TblOrder>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblOrder_pkey");

            entity.ToTable("TblOrder");

            entity.HasIndex(e => e.Status, "idx_order_status");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.AddressCode).HasMaxLength(100);
            entity.Property(e => e.CouponCode).HasMaxLength(100);
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.FinalAmount).HasPrecision(15, 2);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("'pending'::character varying");
            }
            else
            {
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue(OrderStatus.Pending);
            }
            entity.Property(e => e.TotalAmount).HasPrecision(15, 2);
            entity.Property(e => e.UserCode).HasMaxLength(100);

            entity.HasOne(d => d.AddressCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.AddressCode)
                .HasConstraintName("TblOrder_AddressCode_fkey");

            entity.HasOne(d => d.CouponCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.CouponCode)
                .HasConstraintName("TblOrder_CouponCode_fkey");

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblOrder_UserCode_fkey");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(e => e.OrderDate, "idx_order_date");
            entity.HasIndex(e => e.UserCode, "idx_order_user");
        });

        modelBuilder.Entity<TblOrderItem>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblOrderItem_pkey");

            entity.ToTable("TblOrderItem");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.OrderCode).HasMaxLength(100);
            entity.Property(e => e.PriceAtOrder).HasPrecision(15, 2);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasOne(d => d.OrderCodeNavigation).WithMany(p => p.TblOrderItems)
                .HasForeignKey(d => d.OrderCode)
                .HasConstraintName("TblOrderItem_OrderCode_fkey");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.TblOrderItems)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblOrderItem_ProductCode_fkey");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblPayment_pkey");

            entity.ToTable("TblPayment");

            entity.HasIndex(e => e.OrderCode, "TblPayment_OrderCode_key").IsUnique();

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.OrderCode).HasMaxLength(100);
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("'pending'::character varying");
            }
            else
            {
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue(PaymentStatus.Pending);
            }
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("TransactionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasOne(d => d.OrderCodeNavigation).WithOne(p => p.TblPayment)
                .HasForeignKey<TblPayment>(d => d.OrderCode)
                .HasConstraintName("TblPayment_OrderCode_fkey");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProduct_pkey");

            entity.ToTable("TblProduct");



            entity.HasIndex(e => e.Name, "idx_product_name");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.CategoryCode).HasMaxLength(100);
            entity.Property(e => e.CostPrice).HasPrecision(15, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(15, 2);

            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.WholesalePrice).HasPrecision(15, 2);
            entity.Property(e => e.BrandCode).HasMaxLength(50);
            entity.Property(e => e.BaseUnit).HasMaxLength(50);
            entity.Property(e => e.BinLocation).HasMaxLength(100);
            entity.Property(e => e.VatRate).HasPrecision(5, 2);
            entity.Property(e => e.CountryOfOrigin).HasMaxLength(100);

            entity.HasOne(d => d.CategoryCodeNavigation).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.CategoryCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblProduct_CategoryCode_fkey");

            entity.HasOne(d => d.SupplierCodeNavigation).WithMany()
                .HasForeignKey(d => d.SupplierCode)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Brand).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.BrandCode)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasIndex(e => e.CategoryCode, "idx_product_category");
            entity.HasIndex(e => e.SupplierCode, "idx_product_supplier");
            entity.HasIndex(e => e.BrandCode, "idx_product_brand");
            entity.HasIndex(e => e.IsActive, "idx_product_active");
        });



        modelBuilder.Entity<TblProductPromotion>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductPromotion_pkey");

            entity.ToTable("TblProductPromotion");

            entity.HasIndex(e => new { e.ProductCode, e.PromotionCode }, "TblProductPromotion_ProductCode_PromotionCode_key").IsUnique();

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.PromotionCode).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.TblProductPromotions)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblProductPromotion_ProductCode_fkey");

            entity.HasOne(d => d.PromotionCodeNavigation).WithMany(p => p.TblProductPromotions)
                .HasForeignKey(d => d.PromotionCode)
                .HasConstraintName("TblProductPromotion_PromotionCode_fkey");
        });

        modelBuilder.Entity<TblPromotion>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblPromotion_pkey");

            entity.ToTable("TblPromotion");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasPrecision(15, 2);
            entity.Property(e => e.EndDate).HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(15, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(15, 2);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
        });

        modelBuilder.Entity<TblReview>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblReview_pkey");

            entity.ToTable("TblReview");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.OrderItemCode).HasMaxLength(100);
            entity.Property(e => e.UserCode).HasMaxLength(100);

            entity.HasOne(d => d.OrderItemCodeNavigation).WithMany(p => p.TblReviews)
                .HasForeignKey(d => d.OrderItemCode)
                .HasConstraintName("TblReview_OrderItemCode_fkey");

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblReviews)
                .HasForeignKey(d => d.UserCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TblReview_UserCode_fkey");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany()
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblReview_ProductCode_fkey");

            entity.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.ParentCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblReview_ParentCode_fkey");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
        });

        modelBuilder.Entity<TblNews>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblNews_pkey");
            entity.ToTable("TblNews");
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('NWS'::text || lpad((nextval('news_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
        });

        modelBuilder.Entity<TblSupplier>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblSupplier_pkey");

            entity.ToTable("TblSupplier");

            entity.Property(e => e.Code).HasColumnType("character varying");
            entity.Property(e => e.Address).HasColumnType("character varying");
            entity.Property(e => e.BankAccount).HasColumnType("character varying");
            entity.Property(e => e.BankName).HasColumnType("character varying");
            entity.Property(e => e.ContactPerson).HasColumnType("character varying");
            entity.Property(e => e.Email).HasColumnType("character varying");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasColumnType("character varying");
            entity.Property(e => e.Phone).HasColumnType("character varying");
            entity.Property(e => e.TaxCode).HasColumnType("character varying");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<TblQuote>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblQuote_pkey");

            entity.ToTable("TblQuote");

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("'pending'::character varying");
            }
            else
            {
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
            }
            entity.Property(e => e.UserCode).HasMaxLength(100);

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblQuotes)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblQuote_UserCode_fkey");
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
        });


        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblUser_pkey");

            entity.ToTable("TblUser");

            entity.HasIndex(e => e.Email, "TblUser_Email_key").IsUnique();

            entity.HasIndex(e => e.Username, "TblUser_Username_key").IsUnique();

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(15);
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("'customer'::character varying");
            }
            else
            {
                entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue(UserRole.Customer);
            }
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("Add");
            entity.Property(e => e.AvatarUrl).HasMaxLength(1000);
            entity.Property(e => e.RoleCode).HasMaxLength(50);

            entity.HasOne(d => d.RoleCodeNavigation).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.RoleCode)
                .HasConstraintName("TblUser_RoleCode_fkey");

            entity.HasIndex(e => e.Phone, "idx_user_phone");
        });

        modelBuilder.Entity<TblFile>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblFile_pkey");

            entity.ToTable("TblFile");

            entity.HasIndex(e => e.MasterCode, "idx_file_mastercode"); // Add Index

            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('FIL'::text || lpad((nextval('file_code_seq'::regclass))::text, 6, '0'::text))");
            }
            else
            {
                entity.Property(e => e.Code).HasMaxLength(100);
            }
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.OriginalName).HasMaxLength(255);
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Path).HasMaxLength(500);
            entity.Property(e => e.MasterCode).HasMaxLength(100); // Add MasterCode config
            entity.Property(e => e.MasterType).HasMaxLength(50); // Add MasterType config
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
        });
        if (Database.IsNpgsql())
        {
            modelBuilder.HasSequence("address_code_seq");
            modelBuilder.HasSequence("cart_code_seq");
            modelBuilder.HasSequence("cartitem_code_seq");
            modelBuilder.HasSequence("category_code_seq");
            modelBuilder.HasSequence("coupon_code_seq");
            modelBuilder.HasSequence("order_code_seq");
            modelBuilder.HasSequence("orderitem_code_seq");
            modelBuilder.HasSequence("payment_code_seq");
            modelBuilder.HasSequence("product_code_seq");

            modelBuilder.HasSequence("productpromotion_code_seq");
            modelBuilder.HasSequence("promotion_code_seq");
            modelBuilder.HasSequence("review_code_seq");
            modelBuilder.HasSequence("quote_code_seq");
            modelBuilder.HasSequence("user_code_seq");
            modelBuilder.HasSequence("banner_code_seq");
            modelBuilder.HasSequence("file_code_seq");
            modelBuilder.HasSequence("news_code_seq");
        }

        // Brand Configuration
        modelBuilder.Entity<TblBrand>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblBrand_pkey");
            entity.ToTable("TblBrand");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
        });

        // Unit Catalog Configuration (TblUnit)
        modelBuilder.Entity<TblUnit>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblUnit_pkey");
            entity.ToTable("TblUnit");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
        });

        // Product Unit Configuration (TblProductUnit)
        modelBuilder.Entity<TblProductUnit>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductUnit_pkey");
            entity.ToTable("TblProductUnit");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.UnitCode).HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.ConversionRate).HasPrecision(10, 2);
            entity.Property(e => e.IsBaseUnit).HasDefaultValue(false);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductUnits)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblProductUnit_ProductCode_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.TblProductUnits)
                .HasForeignKey(d => d.UnitCode)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("TblProductUnit_UnitCode_fkey");
        });

        // Product Detail Configuration
        modelBuilder.Entity<TblProductDetail>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductDetail_pkey");
            entity.ToTable("TblProductDetail");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.SpecName).HasMaxLength(100);
            entity.Property(e => e.SpecValue).HasMaxLength(255);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");

            entity.Property(e => e.DetailType)
                  .HasMaxLength(50)
                  .HasConversion<EnumToStringConverter<VNVTStore.Domain.Enums.ProductDetailType>>();

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductDetails)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblProductDetail_ProductCode_fkey");
        });

        // Tag Configuration
        modelBuilder.Entity<TblTag>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblTag_pkey");
            entity.ToTable("TblTag");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
        });

        // Product Tag Junction Configuration
        modelBuilder.Entity<TblProductTag>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductTag_pkey");
            entity.ToTable("TblProductTag");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.TagCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductTags)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblProductTag_ProductCode_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.TblProductTags)
                .HasForeignKey(d => d.TagCode)
                .HasConstraintName("TblProductTag_TagCode_fkey");
        });

        // Quote Item Configuration
        modelBuilder.Entity<TblQuoteItem>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblQuoteItem_pkey");
            entity.ToTable("TblQuoteItem");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.QuoteCode).HasMaxLength(100);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.UnitCode).HasMaxLength(50);
            entity.Property(e => e.RequestPrice).HasPrecision(15, 2);
            entity.Property(e => e.ApprovedPrice).HasPrecision(15, 2);
            
            entity.HasOne(d => d.Quote).WithMany(p => p.TblQuoteItems)
                .HasForeignKey(d => d.QuoteCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblQuoteItem_QuoteCode_fkey");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblQuoteItem_ProductCode_fkey");
                
            entity.HasOne(d => d.Unit).WithMany()
                .HasForeignKey(d => d.UnitCode)
                .HasConstraintName("TblQuoteItem_UnitCode_fkey");
        });

        // Debt Log Configuration
        modelBuilder.Entity<TblDebtLog>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblDebtLog_pkey");
            entity.ToTable("TblDebtLog");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.UserCode).HasMaxLength(100);
            entity.Property(e => e.OrderCode).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
            entity.Property(e => e.Reason).HasMaxLength(255);
            
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblDebtLog_UserCode_fkey");
            
            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderCode)
                .HasConstraintName("TblDebtLog_OrderCode_fkey");
        });

        // Product Variant Configuration
        modelBuilder.Entity<TblProductVariant>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductVariant_pkey");
            entity.ToTable("TblProductVariant");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");

            entity.HasOne(d => d.Product).WithMany(p => p.TblProductVariants)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblProductVariant_ProductCode_fkey");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblRole_pkey");
            entity.ToTable("TblRole");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblPermission_pkey");
            entity.ToTable("TblPermission");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        modelBuilder.Entity<TblRolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleCode, e.PermissionCode }).HasName("TblRolePermission_pkey");
            entity.ToTable("TblRolePermission");
            entity.HasOne(d => d.RoleCodeNavigation).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.RoleCode)
                .HasConstraintName("TblRolePermission_RoleCode_fkey");
            entity.HasOne(d => d.PermissionCodeNavigation).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.PermissionCode)
                .HasConstraintName("TblRolePermission_PermissionCode_fkey");
        });





        // User Login Configuration
        modelBuilder.Entity<TblUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey }).HasName("TblUserLogin_pkey");
            entity.ToTable("TblUserLogin");
            
            entity.Property(e => e.LoginProvider).HasMaxLength(50);
            entity.Property(e => e.ProviderKey).HasMaxLength(255);
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.TblUserLogins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblUserLogin_UserId_fkey");
        });

        // Supplier Configuration
        modelBuilder.Entity<TblSupplier>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblSupplier_pkey");
            entity.ToTable("TblSupplier");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
        });

        // System Config Configuration
        modelBuilder.Entity<TblSystemConfig>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblSystemConfig_pkey");
            entity.ToTable("TblSystemConfig");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ConfigValue).IsRequired(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedType).HasDefaultValue("ADD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp with time zone");
        });

        // Menu Configuration
        modelBuilder.Entity<TblMenu>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblMenu_pkey");
            entity.ToTable("TblMenu");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Path).HasMaxLength(255);
            entity.Property(e => e.GroupCode).HasMaxLength(50);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // RoleMenu Configuration (Junction Table)
        modelBuilder.Entity<TblRoleMenu>(entity =>
        {
            entity.HasKey(e => new { e.RoleCode, e.MenuCode }).HasName("TblRoleMenu_pkey");
            entity.ToTable("TblRoleMenu");
            entity.Property(e => e.RoleCode).HasMaxLength(50);
            entity.Property(e => e.MenuCode).HasMaxLength(50);

            entity.HasOne(d => d.RoleCodeNavigation)
                .WithMany(p => p.TblRoleMenus)
                .HasForeignKey(d => d.RoleCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblRoleMenu_RoleCode_fkey");

            entity.HasOne(d => d.MenuCodeNavigation)
                .WithMany(p => p.TblRoleMenus)
                .HasForeignKey(d => d.MenuCode)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("TblRoleMenu_MenuCode_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

// Custom Value Converter for Lowercase Enums
public class EnumLowerCaseConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
{
    public EnumLowerCaseConverter() : base(
        v => v.ToString().ToLower(),
        v => Enum.Parse<TEnum>(v, true))
    { }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        v => v.HasValue ? v.Value.ToUniversalTime() : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    { }
}
