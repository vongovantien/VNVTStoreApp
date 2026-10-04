using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Role enum column was already removed in a previous migration or never existed
            // with this exact name — skip the DropColumn to avoid failure on fresh DBs.
            // migrationBuilder.DropColumn(name: "Role", table: "TblUser");

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "TblUser",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "CUSTOMER",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblUnit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblUnit",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblUnit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblTag",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblTag",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblTag",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblSystemSecret",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblSystemSecret",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblSystemSecret",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblSystemConfig",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblSystemConfig",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblSystemConfig",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblSupplier",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblSupplier",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblSupplier",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblRole",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblRole",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblRole",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblQuoteItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblQuoteItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblQuoteItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblQuote",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblQuote",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblQuote",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblPromotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblPromotion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblPromotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblProductTag",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblProductTag",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblProductTag",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblProductPromotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblProductPromotion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblProductPromotion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContactPrice",
                table: "TblProduct",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblProduct",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblPayment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblPayment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblOrderItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblOrder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblOrder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblOrder",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedType",
                table: "TblNotification",
                type: "text",
                nullable: true,
                defaultValue: "ADD",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldDefaultValue: "ADD");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblNews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblNews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblNews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblDelivery",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblDelivery",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblDelivery",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblDebtLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblDebtLog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblDebtLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblCategory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblCategory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblCartItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblCartItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblCartItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblCart",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblCart",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblCart",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblBrand",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblBrand",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblBrand",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedType",
                table: "TblAuditLog",
                type: "text",
                nullable: true,
                defaultValue: "ADD",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldDefaultValue: "ADD");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TblAddress",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixed",
                table: "TblAddress",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TblAddress",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TblPaymentMethods",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblPaymentMethods", x => x.Code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblPaymentMethods");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblUnit");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblUnit");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblUnit");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblTag");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblTag");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblTag");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblSystemSecret");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblSystemSecret");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblSystemSecret");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblSystemConfig");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblSystemConfig");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblSystemConfig");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblSupplier");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblSupplier");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblSupplier");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblRole");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblRole");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblRole");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblQuoteItem");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblQuoteItem");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblQuoteItem");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblPromotion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblProductTag");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblProductTag");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblProductTag");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblProductPromotion");

            migrationBuilder.DropColumn(
                name: "ContactPrice",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblPayment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblNews");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblNews");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblNews");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblDelivery");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblDelivery");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblDelivery");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblDebtLog");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblDebtLog");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblDebtLog");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblCart");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblCart");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblCart");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblBrand");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblBrand");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblBrand");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TblAddress");

            migrationBuilder.DropColumn(
                name: "IsFixed",
                table: "TblAddress");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TblAddress");

            migrationBuilder.AlterColumn<string>(
                name: "RoleCode",
                table: "TblUser",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "CUSTOMER");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "TblUser",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'customer'::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedType",
                table: "TblNotification",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "ADD",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "ADD");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedType",
                table: "TblAuditLog",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "ADD",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "ADD");
        }
    }
}
