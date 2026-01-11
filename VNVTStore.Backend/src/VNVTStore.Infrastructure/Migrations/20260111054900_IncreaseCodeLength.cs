using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseCodeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblUser",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblReview",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "OrderItemCode",
                table: "TblReview",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblReview",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblQuote",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblQuote",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblQuote",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblPromotion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "PromotionCode",
                table: "TblProductPromotion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblProductPromotion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProductPromotion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblProductImage",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProductImage",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryCode",
                table: "TblProduct",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProduct",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "TblPayment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblPayment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblOrderItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "TblOrderItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblOrderItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblOrder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "TblOrder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressCode",
                table: "TblOrder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblOrder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "PromotionCode",
                table: "TblCoupon",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCoupon",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "TblCategory",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCategory",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblCartItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "CartCode",
                table: "TblCartItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCartItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblCart",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCart",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblBanner",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblAddress",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "TblAddress",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblAddress",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblUser",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblReview",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "OrderItemCode",
                table: "TblReview",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblReview",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblQuote",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblQuote",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblQuote",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblPromotion",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "PromotionCode",
                table: "TblProductPromotion",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblProductPromotion",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProductPromotion",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblProductImage",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProductImage",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryCode",
                table: "TblProduct",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblProduct",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "TblPayment",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblPayment",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblOrderItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "TblOrderItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblOrderItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblOrder",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "TblOrder",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressCode",
                table: "TblOrder",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblOrder",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "PromotionCode",
                table: "TblCoupon",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCoupon",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "TblCategory",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCategory",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblCartItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CartCode",
                table: "TblCartItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCartItem",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblCart",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblCart",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblBanner",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblAddress",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "TblAddress",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TblAddress",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))");
        }
    }
}
