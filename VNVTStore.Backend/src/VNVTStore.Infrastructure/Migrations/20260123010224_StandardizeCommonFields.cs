using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeCommonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
/*
            migrationBuilder.RenameIndex(
                name: "IX_TblProduct_SupplierCode",
                table: "TblProduct",
                newName: "idx_product_supplier");

            migrationBuilder.RenameIndex(
                name: "IX_TblProduct_CategoryCode",
                table: "TblProduct",
                newName: "idx_product_category");

            migrationBuilder.RenameIndex(
                name: "IX_TblOrder_UserCode",
                table: "TblOrder",
                newName: "idx_order_user");

            migrationBuilder.RenameIndex(
                name: "IX_TblCategory_ParentCode",
                table: "TblCategory",
                newName: "idx_category_parent");

            migrationBuilder.RenameIndex(
                name: "IX_TblAddress_UserCode",
                table: "TblAddress",
                newName: "idx_address_user");
*/

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblUser",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblSupplier",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblSupplier",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblReview",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblReview",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblQuote",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblQuote",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblPromotion",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblPromotion",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblPromotion",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblPromotion",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblProductPromotion",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblProductPromotion",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblProductPromotion",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblProductPromotion",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblProduct",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblPayment",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblPayment",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblPayment",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblPayment",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblOrderItem",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblOrderItem",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblOrderItem",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblOrderItem",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblOrder",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblOrder",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblOrder",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblCoupon",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCartItem",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblCartItem",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblCartItem",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCartItem",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblCart",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblCart",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblBanner",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblAddress",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblAddress",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblAddress",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "idx_user_phone",
                table: "TblUser",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "idx_product_active",
                table: "TblProduct",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_product_code",
                table: "TblProduct",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "idx_order_date",
                table: "TblOrder",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "idx_category_name",
                table: "TblCategory",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_user_phone",
                table: "TblUser");

            migrationBuilder.DropIndex(
                name: "idx_product_active",
                table: "TblProduct");

            migrationBuilder.DropIndex(
                name: "idx_product_code",
                table: "TblProduct");

            migrationBuilder.DropIndex(
                name: "idx_order_date",
                table: "TblOrder");

            migrationBuilder.DropIndex(
                name: "idx_category_name",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblCart");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblCart");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblAddress");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblAddress");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblAddress");

/*
            migrationBuilder.RenameIndex(
                name: "idx_product_supplier",
                table: "TblProduct",
                newName: "IX_TblProduct_SupplierCode");

            migrationBuilder.RenameIndex(
                name: "idx_product_category",
                table: "TblProduct",
                newName: "IX_TblProduct_CategoryCode");

            migrationBuilder.RenameIndex(
                name: "idx_order_user",
                table: "TblOrder",
                newName: "IX_TblOrder_UserCode");

            migrationBuilder.RenameIndex(
                name: "idx_category_parent",
                table: "TblCategory",
                newName: "IX_TblCategory_ParentCode");

            migrationBuilder.RenameIndex(
                name: "idx_address_user",
                table: "TblAddress",
                newName: "IX_TblAddress_UserCode");
*/

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblUser",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblSupplier",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblSupplier",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblReview",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblReview",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblQuote",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblQuote",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblPromotion",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblProduct",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblCoupon",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblBanner",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
