using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FreshInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "address_code_seq");

            migrationBuilder.CreateSequence(
                name: "banner_code_seq");

            migrationBuilder.CreateSequence(
                name: "cart_code_seq");

            migrationBuilder.CreateSequence(
                name: "cartitem_code_seq");

            migrationBuilder.CreateSequence(
                name: "category_code_seq");

            migrationBuilder.CreateSequence(
                name: "coupon_code_seq");

            migrationBuilder.CreateSequence(
                name: "file_code_seq");

            migrationBuilder.CreateSequence(
                name: "news_code_seq");

            migrationBuilder.CreateSequence(
                name: "order_code_seq");

            migrationBuilder.CreateSequence(
                name: "orderitem_code_seq");

            migrationBuilder.CreateSequence(
                name: "payment_code_seq");

            migrationBuilder.CreateSequence(
                name: "product_code_seq");

            migrationBuilder.CreateSequence(
                name: "productpromotion_code_seq");

            migrationBuilder.CreateSequence(
                name: "promotion_code_seq");

            migrationBuilder.CreateSequence(
                name: "quote_code_seq");

            migrationBuilder.CreateSequence(
                name: "review_code_seq");

            migrationBuilder.CreateSequence(
                name: "user_code_seq");

            migrationBuilder.CreateTable(
                name: "TblBanner",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LinkText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblBanner_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblBrand",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblBrand_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblCategory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageURL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblCategory_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblCategory_ParentCode_fkey",
                        column: x => x.ParentCode,
                        principalTable: "TblCategory",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TblFile",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('FIL'::text || lpad((nextval('file_code_seq'::regclass))::text, 6, '0'::text))"),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    MasterCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MasterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblFile_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblNews",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('NWS'::text || lpad((nextval('news_code_seq'::regclass))::text, 6, '0'::text))"),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Thumbnail = table.Column<string>(type: "text", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MetaTitle = table.Column<string>(type: "text", nullable: true),
                    MetaDescription = table.Column<string>(type: "text", nullable: true),
                    MetaKeywords = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblNews_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblPermission",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblPermission_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblPromotion",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblPromotion_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblRole",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblRole_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblSupplier",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying", nullable: false),
                    Name = table.Column<string>(type: "character varying", nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying", nullable: true),
                    Email = table.Column<string>(type: "character varying", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    Phone = table.Column<string>(type: "character varying", nullable: true),
                    Address = table.Column<string>(type: "character varying", nullable: true),
                    TaxCode = table.Column<string>(type: "character varying", nullable: true),
                    BankAccount = table.Column<string>(type: "character varying", nullable: true),
                    BankName = table.Column<string>(type: "character varying", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblSupplier_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblTag",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblTag_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblUnit",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblUnit_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblCoupon",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))"),
                    PromotionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblCoupon_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblCoupon_PromotionCode_fkey",
                        column: x => x.PromotionCode,
                        principalTable: "TblPromotion",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TblRolePermission",
                columns: table => new
                {
                    RoleCode = table.Column<string>(type: "character varying(50)", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblRolePermission_pkey", x => new { x.RoleCode, x.PermissionCode });
                    table.ForeignKey(
                        name: "TblRolePermission_PermissionCode_fkey",
                        column: x => x.PermissionCode,
                        principalTable: "TblPermission",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblRolePermission_RoleCode_fkey",
                        column: x => x.RoleCode,
                        principalTable: "TblRole",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblUser",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))"),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    RoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'customer'::character varying"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "Add"),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoyaltyPoints = table.Column<int>(type: "integer", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DebtLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentDebt = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblUser_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblUser_RoleCode_fkey",
                        column: x => x.RoleCode,
                        principalTable: "TblRole",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TblProduct",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    WholesalePrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    CostPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CategoryCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    SupplierCode = table.Column<string>(type: "character varying", nullable: true),
                    BrandCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MinStockLevel = table.Column<int>(type: "integer", nullable: true),
                    BinLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VatRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProduct_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "FK_TblProduct_TblBrand_BrandCode",
                        column: x => x.BrandCode,
                        principalTable: "TblBrand",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TblProduct_TblSupplier_SupplierCode",
                        column: x => x.SupplierCode,
                        principalTable: "TblSupplier",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "TblProduct_CategoryCode_fkey",
                        column: x => x.CategoryCode,
                        principalTable: "TblCategory",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TblAddress",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValueSql: "'Vietnam'::character varying"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblAddress_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblAddress_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblCart",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblCart_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblCart_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblUserLogin",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblUserLogin_pkey", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "TblUserLogin_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblProductDetail",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DetailType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SpecName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpecValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductDetail_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductDetail_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblProductPromotion",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))"),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromotionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductPromotion_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductPromotion_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblProductPromotion_PromotionCode_fkey",
                        column: x => x.PromotionCode,
                        principalTable: "TblPromotion",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblProductTag",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TagCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductTag_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductTag_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblProductTag_TagCode_fkey",
                        column: x => x.TagCode,
                        principalTable: "TblTag",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblProductUnit",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductUnit_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductUnit_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblProductUnit_UnitCode_fkey",
                        column: x => x.UnitCode,
                        principalTable: "TblUnit",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TblProductVariant",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Attributes = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductVariant_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductVariant_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblQuote",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerEmail = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AdminNote = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    TblProductCode = table.Column<string>(type: "character varying(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblQuote_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "FK_TblQuote_TblProduct_TblProductCode",
                        column: x => x.TblProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "TblQuote_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TblOrder",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TotalAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "0"),
                    FinalAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying"),
                    AddressCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CouponCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerificationToken = table.Column<string>(type: "text", nullable: true),
                    VerificationTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblOrder_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblOrder_AddressCode_fkey",
                        column: x => x.AddressCode,
                        principalTable: "TblAddress",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "TblOrder_CouponCode_fkey",
                        column: x => x.CouponCode,
                        principalTable: "TblCoupon",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "TblOrder_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblCartItem",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))"),
                    CartCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Size = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblCartItem_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblCartItem_CartCode_fkey",
                        column: x => x.CartCode,
                        principalTable: "TblCart",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblCartItem_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblQuoteItem",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuoteCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RequestPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ApprovedPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblQuoteItem_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblQuoteItem_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblQuoteItem_QuoteCode_fkey",
                        column: x => x.QuoteCode,
                        principalTable: "TblQuote",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblQuoteItem_UnitCode_fkey",
                        column: x => x.UnitCode,
                        principalTable: "TblUnit",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TblDebtLog",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblDebtLog_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblDebtLog_OrderCode_fkey",
                        column: x => x.OrderCode,
                        principalTable: "TblOrder",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "TblDebtLog_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblOrderItem",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    ProductImage = table.Column<string>(type: "text", nullable: true),
                    PriceAtOrder = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "0"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblOrderItem_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblOrderItem_OrderCode_fkey",
                        column: x => x.OrderCode,
                        principalTable: "TblOrder",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblOrderItem_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TblPayment",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblPayment_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblPayment_OrderCode_fkey",
                        column: x => x.OrderCode,
                        principalTable: "TblOrder",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblReview",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AdminReply = table.Column<string>(type: "text", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblReview_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblReview_OrderItemCode_fkey",
                        column: x => x.OrderItemCode,
                        principalTable: "TblOrderItem",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "TblReview_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "idx_address_user",
                table: "TblAddress",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "idx_cart_user",
                table: "TblCart",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblCartItem_CartCode",
                table: "TblCartItem",
                column: "CartCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblCartItem_ProductCode",
                table: "TblCartItem",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "idx_category_name",
                table: "TblCategory",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "idx_category_parent",
                table: "TblCategory",
                column: "ParentCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblCoupon_PromotionCode",
                table: "TblCoupon",
                column: "PromotionCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblDebtLog_OrderCode",
                table: "TblDebtLog",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblDebtLog_UserCode",
                table: "TblDebtLog",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "idx_file_mastercode",
                table: "TblFile",
                column: "MasterCode");

            migrationBuilder.CreateIndex(
                name: "idx_order_date",
                table: "TblOrder",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "idx_order_status",
                table: "TblOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_order_user",
                table: "TblOrder",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrder_AddressCode",
                table: "TblOrder",
                column: "AddressCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrder_CouponCode",
                table: "TblOrder",
                column: "CouponCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrderItem_OrderCode",
                table: "TblOrderItem",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrderItem_ProductCode",
                table: "TblOrderItem",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "TblPayment_OrderCode_key",
                table: "TblPayment",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_product_active",
                table: "TblProduct",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_product_brand",
                table: "TblProduct",
                column: "BrandCode");

            migrationBuilder.CreateIndex(
                name: "idx_product_category",
                table: "TblProduct",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "idx_product_name",
                table: "TblProduct",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "idx_product_supplier",
                table: "TblProduct",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductDetail_ProductCode",
                table: "TblProductDetail",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductPromotion_PromotionCode",
                table: "TblProductPromotion",
                column: "PromotionCode");

            migrationBuilder.CreateIndex(
                name: "TblProductPromotion_ProductCode_PromotionCode_key",
                table: "TblProductPromotion",
                columns: new[] { "ProductCode", "PromotionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblProductTag_ProductCode",
                table: "TblProductTag",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductTag_TagCode",
                table: "TblProductTag",
                column: "TagCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductUnit_ProductCode",
                table: "TblProductUnit",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductUnit_UnitCode",
                table: "TblProductUnit",
                column: "UnitCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductVariant_ProductCode",
                table: "TblProductVariant",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuote_TblProductCode",
                table: "TblQuote",
                column: "TblProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuote_UserCode",
                table: "TblQuote",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuoteItem_ProductCode",
                table: "TblQuoteItem",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuoteItem_QuoteCode",
                table: "TblQuoteItem",
                column: "QuoteCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuoteItem_UnitCode",
                table: "TblQuoteItem",
                column: "UnitCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_OrderItemCode",
                table: "TblReview",
                column: "OrderItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_UserCode",
                table: "TblReview",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblRolePermission_PermissionCode",
                table: "TblRolePermission",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "idx_user_phone",
                table: "TblUser",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_TblUser_RoleCode",
                table: "TblUser",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "TblUser_Email_key",
                table: "TblUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "TblUser_Username_key",
                table: "TblUser",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblUserLogin_UserId",
                table: "TblUserLogin",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblBanner");

            migrationBuilder.DropTable(
                name: "TblCartItem");

            migrationBuilder.DropTable(
                name: "TblDebtLog");

            migrationBuilder.DropTable(
                name: "TblFile");

            migrationBuilder.DropTable(
                name: "TblNews");

            migrationBuilder.DropTable(
                name: "TblPayment");

            migrationBuilder.DropTable(
                name: "TblProductDetail");

            migrationBuilder.DropTable(
                name: "TblProductPromotion");

            migrationBuilder.DropTable(
                name: "TblProductTag");

            migrationBuilder.DropTable(
                name: "TblProductUnit");

            migrationBuilder.DropTable(
                name: "TblProductVariant");

            migrationBuilder.DropTable(
                name: "TblQuoteItem");

            migrationBuilder.DropTable(
                name: "TblReview");

            migrationBuilder.DropTable(
                name: "TblRolePermission");

            migrationBuilder.DropTable(
                name: "TblUserLogin");

            migrationBuilder.DropTable(
                name: "TblCart");

            migrationBuilder.DropTable(
                name: "TblTag");

            migrationBuilder.DropTable(
                name: "TblQuote");

            migrationBuilder.DropTable(
                name: "TblUnit");

            migrationBuilder.DropTable(
                name: "TblOrderItem");

            migrationBuilder.DropTable(
                name: "TblPermission");

            migrationBuilder.DropTable(
                name: "TblOrder");

            migrationBuilder.DropTable(
                name: "TblProduct");

            migrationBuilder.DropTable(
                name: "TblAddress");

            migrationBuilder.DropTable(
                name: "TblCoupon");

            migrationBuilder.DropTable(
                name: "TblBrand");

            migrationBuilder.DropTable(
                name: "TblSupplier");

            migrationBuilder.DropTable(
                name: "TblCategory");

            migrationBuilder.DropTable(
                name: "TblUser");

            migrationBuilder.DropTable(
                name: "TblPromotion");

            migrationBuilder.DropTable(
                name: "TblRole");

            migrationBuilder.DropSequence(
                name: "address_code_seq");

            migrationBuilder.DropSequence(
                name: "banner_code_seq");

            migrationBuilder.DropSequence(
                name: "cart_code_seq");

            migrationBuilder.DropSequence(
                name: "cartitem_code_seq");

            migrationBuilder.DropSequence(
                name: "category_code_seq");

            migrationBuilder.DropSequence(
                name: "coupon_code_seq");

            migrationBuilder.DropSequence(
                name: "file_code_seq");

            migrationBuilder.DropSequence(
                name: "news_code_seq");

            migrationBuilder.DropSequence(
                name: "order_code_seq");

            migrationBuilder.DropSequence(
                name: "orderitem_code_seq");

            migrationBuilder.DropSequence(
                name: "payment_code_seq");

            migrationBuilder.DropSequence(
                name: "product_code_seq");

            migrationBuilder.DropSequence(
                name: "productpromotion_code_seq");

            migrationBuilder.DropSequence(
                name: "promotion_code_seq");

            migrationBuilder.DropSequence(
                name: "quote_code_seq");

            migrationBuilder.DropSequence(
                name: "review_code_seq");

            migrationBuilder.DropSequence(
                name: "user_code_seq");
        }
    }
}
