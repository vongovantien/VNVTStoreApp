using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop all tables and sequences to ensure clean slate for InitialCreate
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblCartItem\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblPayment\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblProductImage\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblProductPromotion\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblQuote\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblReview\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblCart\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblOrderItem\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblOrder\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblProduct\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblAddress\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblCoupon\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblSupplier\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblCategory\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblUser\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TblPromotion\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE;"); 

            // Drop sequences if they exist
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"address_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"cart_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"cartitem_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"category_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"coupon_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"order_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"orderitem_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"payment_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"product_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"productimage_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"productpromotion_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"promotion_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"quote_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"review_code_seq\" CASCADE;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"user_code_seq\" CASCADE;");

            migrationBuilder.CreateSequence(
                name: "address_code_seq");

            migrationBuilder.CreateSequence(
                name: "cart_code_seq");

            migrationBuilder.CreateSequence(
                name: "cartitem_code_seq");

            migrationBuilder.CreateSequence(
                name: "category_code_seq");

            migrationBuilder.CreateSequence(
                name: "coupon_code_seq");

            migrationBuilder.CreateSequence(
                name: "order_code_seq");

            migrationBuilder.CreateSequence(
                name: "orderitem_code_seq");

            migrationBuilder.CreateSequence(
                name: "payment_code_seq");

            migrationBuilder.CreateSequence(
                name: "product_code_seq");

            migrationBuilder.CreateSequence(
                name: "productimage_code_seq");

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
                name: "TblCategory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ImageURL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
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
                name: "TblPromotion",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblPromotion_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblSupplier",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying", nullable: false),
                    Name = table.Column<string>(type: "character varying", nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying", nullable: true),
                    Email = table.Column<string>(type: "character varying", nullable: true),
                    Phone = table.Column<string>(type: "character varying", nullable: true),
                    Address = table.Column<string>(type: "character varying", nullable: true),
                    TaxCode = table.Column<string>(type: "character varying", nullable: true),
                    BankAccount = table.Column<string>(type: "character varying", nullable: true),
                    BankName = table.Column<string>(type: "character varying", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblSupplier_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblUser",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))"),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'customer'::character varying"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblUser_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblCoupon",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))"),
                    PromotionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
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
                name: "TblProduct",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CategoryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SupplierCode = table.Column<string>(type: "text", nullable: true),
                    SupplierCodeNavigationCode = table.Column<string>(type: "character varying", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProduct_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "FK_TblProduct_TblSupplier_SupplierCodeNavigationCode",
                        column: x => x.SupplierCodeNavigationCode,
                        principalTable: "TblSupplier",
                        principalColumn: "Code");
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
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValueSql: "'Vietnam'::character varying"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "TblProductImage",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))"),
                    ProductCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ImageURL = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AltText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblProductImage_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblProductImage_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblProductPromotion",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))"),
                    ProductCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PromotionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
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
                name: "TblQuote",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Note = table.Column<string>(type: "text", nullable: true),
                    QuotedPrice = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    AdminNote = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblQuote_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblQuote_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblQuote_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblOrder",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TotalAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "0"),
                    FinalAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'pending'::character varying"),
                    AddressCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CouponCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
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
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))"),
                    CartCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AddedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "TblOrderItem",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PriceAtOrder = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true, defaultValueSql: "0")
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
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValueSql: "'pending'::character varying")
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
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderItemCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UserCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
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
                name: "IX_TblAddress_UserCode",
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
                name: "IX_TblCategory_ParentCode",
                table: "TblCategory",
                column: "ParentCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblCoupon_PromotionCode",
                table: "TblCoupon",
                column: "PromotionCode");

            migrationBuilder.CreateIndex(
                name: "idx_order_status",
                table: "TblOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrder_AddressCode",
                table: "TblOrder",
                column: "AddressCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrder_CouponCode",
                table: "TblOrder",
                column: "CouponCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblOrder_UserCode",
                table: "TblOrder",
                column: "UserCode");

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
                name: "idx_product_name",
                table: "TblProduct",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TblProduct_CategoryCode",
                table: "TblProduct",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProduct_SupplierCodeNavigationCode",
                table: "TblProduct",
                column: "SupplierCodeNavigationCode");

            migrationBuilder.CreateIndex(
                name: "TblProduct_SKU_key",
                table: "TblProduct",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblProductImage_ProductCode",
                table: "TblProductImage",
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
                name: "IX_TblQuote_ProductCode",
                table: "TblQuote",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblQuote_UserCode",
                table: "TblQuote",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_OrderItemCode",
                table: "TblReview",
                column: "OrderItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_UserCode",
                table: "TblReview",
                column: "UserCode");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblCartItem");

            migrationBuilder.DropTable(
                name: "TblPayment");

            migrationBuilder.DropTable(
                name: "TblProductImage");

            migrationBuilder.DropTable(
                name: "TblProductPromotion");

            migrationBuilder.DropTable(
                name: "TblQuote");

            migrationBuilder.DropTable(
                name: "TblReview");

            migrationBuilder.DropTable(
                name: "TblCart");

            migrationBuilder.DropTable(
                name: "TblOrderItem");

            migrationBuilder.DropTable(
                name: "TblOrder");

            migrationBuilder.DropTable(
                name: "TblProduct");

            migrationBuilder.DropTable(
                name: "TblAddress");

            migrationBuilder.DropTable(
                name: "TblCoupon");

            migrationBuilder.DropTable(
                name: "TblSupplier");

            migrationBuilder.DropTable(
                name: "TblCategory");

            migrationBuilder.DropTable(
                name: "TblUser");

            migrationBuilder.DropTable(
                name: "TblPromotion");

            migrationBuilder.DropSequence(
                name: "address_code_seq");

            migrationBuilder.DropSequence(
                name: "cart_code_seq");

            migrationBuilder.DropSequence(
                name: "cartitem_code_seq");

            migrationBuilder.DropSequence(
                name: "category_code_seq");

            migrationBuilder.DropSequence(
                name: "coupon_code_seq");

            migrationBuilder.DropSequence(
                name: "order_code_seq");

            migrationBuilder.DropSequence(
                name: "orderitem_code_seq");

            migrationBuilder.DropSequence(
                name: "payment_code_seq");

            migrationBuilder.DropSequence(
                name: "product_code_seq");

            migrationBuilder.DropSequence(
                name: "productimage_code_seq");

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
