using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VNVTStore.Infrastructure;

namespace VNVTStore.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAddress> TblAddresses { get; set; }

    public virtual DbSet<TblCart> TblCarts { get; set; }

    public virtual DbSet<TblCartItem> TblCartItems { get; set; }

    public virtual DbSet<TblCategory> TblCategories { get; set; }

    public virtual DbSet<TblCoupon> TblCoupons { get; set; }

    public virtual DbSet<TblOrder> TblOrders { get; set; }

    public virtual DbSet<TblOrderItem> TblOrderItems { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblProductImage> TblProductImages { get; set; }

    public virtual DbSet<TblProductPromotion> TblProductPromotions { get; set; }

    public virtual DbSet<TblPromotion> TblPromotions { get; set; }

    public virtual DbSet<TblReview> TblReviews { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAddress>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblAddress_pkey");

            entity.ToTable("TblAddress");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.AddressLine).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Vietnam'::character varying");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.UserCode).HasMaxLength(10);

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblAddresses)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblAddress_UserCode_fkey");
        });

        modelBuilder.Entity<TblCart>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCart_pkey");

            entity.ToTable("TblCart");

            entity.HasIndex(e => e.UserCode, "idx_cart_user");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserCode).HasMaxLength(10);

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblCarts)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblCart_UserCode_fkey");
        });

        modelBuilder.Entity<TblCartItem>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCartItem_pkey");

            entity.ToTable("TblCartItem");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CartCode).HasMaxLength(10);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

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

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ParentCode).HasMaxLength(10);

            entity.HasOne(d => d.ParentCodeNavigation).WithMany(p => p.InverseParentCodeNavigation)
                .HasForeignKey(d => d.ParentCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblCategory_ParentCode_fkey");
        });

        modelBuilder.Entity<TblCoupon>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblCoupon_pkey");

            entity.ToTable("TblCoupon");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.PromotionCode).HasMaxLength(10);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);

            entity.HasOne(d => d.PromotionCodeNavigation).WithMany(p => p.TblCoupons)
                .HasForeignKey(d => d.PromotionCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblCoupon_PromotionCode_fkey");
        });

        modelBuilder.Entity<TblOrder>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblOrder_pkey");

            entity.ToTable("TblOrder");

            entity.HasIndex(e => e.Status, "idx_order_status");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.AddressCode).HasMaxLength(10);
            entity.Property(e => e.CouponCode).HasMaxLength(10);
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.FinalAmount).HasPrecision(15, 2);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.TotalAmount).HasPrecision(15, 2);
            entity.Property(e => e.UserCode).HasMaxLength(10);

            entity.HasOne(d => d.AddressCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.AddressCode)
                .HasConstraintName("TblOrder_AddressCode_fkey");

            entity.HasOne(d => d.CouponCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.CouponCode)
                .HasConstraintName("TblOrder_CouponCode_fkey");

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblOrders)
                .HasForeignKey(d => d.UserCode)
                .HasConstraintName("TblOrder_UserCode_fkey");
        });

        modelBuilder.Entity<TblOrderItem>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblOrderItem_pkey");

            entity.ToTable("TblOrderItem");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(15, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.OrderCode).HasMaxLength(10);
            entity.Property(e => e.PriceAtOrder).HasPrecision(15, 2);
            entity.Property(e => e.ProductCode).HasMaxLength(10);

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

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.OrderCode).HasMaxLength(10);
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("TransactionID");

            entity.HasOne(d => d.OrderCodeNavigation).WithOne(p => p.TblPayment)
                .HasForeignKey<TblPayment>(d => d.OrderCode)
                .HasConstraintName("TblPayment_OrderCode_fkey");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProduct_pkey");

            entity.ToTable("TblProduct");

            entity.HasIndex(e => e.Sku, "TblProduct_SKU_key").IsUnique();

            entity.HasIndex(e => e.Name, "idx_product_name");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.CategoryCode).HasMaxLength(10);
            entity.Property(e => e.CostPrice).HasPrecision(15, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.Weight).HasPrecision(8, 2);

            entity.HasOne(d => d.CategoryCodeNavigation).WithMany(p => p.TblProducts)
                .HasForeignKey(d => d.CategoryCode)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("TblProduct_CategoryCode_fkey");
        });

        modelBuilder.Entity<TblProductImage>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductImage_pkey");

            entity.ToTable("TblProductImage");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.AltText).HasMaxLength(255);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.TblProductImages)
                .HasForeignKey(d => d.ProductCode)
                .HasConstraintName("TblProductImage_ProductCode_fkey");
        });

        modelBuilder.Entity<TblProductPromotion>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblProductPromotion_pkey");

            entity.ToTable("TblProductPromotion");

            entity.HasIndex(e => new { e.ProductCode, e.PromotionCode }, "TblProductPromotion_ProductCode_PromotionCode_key").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.ProductCode).HasMaxLength(10);
            entity.Property(e => e.PromotionCode).HasMaxLength(10);

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

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasPrecision(15, 2);
            entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(15, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(15, 2);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TblReview>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblReview_pkey");

            entity.ToTable("TblReview");

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.OrderItemCode).HasMaxLength(10);
            entity.Property(e => e.UserCode).HasMaxLength(10);

            entity.HasOne(d => d.OrderItemCodeNavigation).WithMany(p => p.TblReviews)
                .HasForeignKey(d => d.OrderItemCode)
                .HasConstraintName("TblReview_OrderItemCode_fkey");

            entity.HasOne(d => d.UserCodeNavigation).WithMany(p => p.TblReviews)
                .HasForeignKey(d => d.UserCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TblReview_UserCode_fkey");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("TblUser_pkey");

            entity.ToTable("TblUser");

            entity.HasIndex(e => e.Email, "TblUser_Email_key").IsUnique();

            entity.HasIndex(e => e.Username, "TblUser_Username_key").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasDefaultValueSql("('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'customer'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Username).HasMaxLength(50);
        });
        modelBuilder.HasSequence("address_code_seq");
        modelBuilder.HasSequence("cart_code_seq");
        modelBuilder.HasSequence("cartitem_code_seq");
        modelBuilder.HasSequence("category_code_seq");
        modelBuilder.HasSequence("coupon_code_seq");
        modelBuilder.HasSequence("order_code_seq");
        modelBuilder.HasSequence("orderitem_code_seq");
        modelBuilder.HasSequence("payment_code_seq");
        modelBuilder.HasSequence("product_code_seq");
        modelBuilder.HasSequence("productimage_code_seq");
        modelBuilder.HasSequence("productpromotion_code_seq");
        modelBuilder.HasSequence("promotion_code_seq");
        modelBuilder.HasSequence("review_code_seq");
        modelBuilder.HasSequence("user_code_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
