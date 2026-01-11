using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_TblProduct_TblSupplier_SupplierCodeNavigationCode",
            //     table: "TblProduct");

            // migrationBuilder.DropIndex(
            //     name: "IX_TblProduct_SupplierCodeNavigationCode",
            //     table: "TblProduct");

            // migrationBuilder.DropColumn(
            //     name: "SupplierCodeNavigationCode",
            //     table: "TblProduct");

            migrationBuilder.CreateSequence(
                name: "banner_code_seq");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierCode",
                table: "TblProduct",
                type: "character varying",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            // migrationBuilder.AddColumn<decimal>(
            //     name: "ShippingFee",
            //     table: "TblOrder",
            //     type: "numeric",
            //     nullable: false,
            //     defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TblCartItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "TblCartItem",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TblBanner",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LinkText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblBanner_pkey", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblProduct_SupplierCode",
                table: "TblProduct",
                column: "SupplierCode");

            migrationBuilder.AddForeignKey(
                name: "FK_TblProduct_TblSupplier_SupplierCode",
                table: "TblProduct",
                column: "SupplierCode",
                principalTable: "TblSupplier",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblProduct_TblSupplier_SupplierCode",
                table: "TblProduct");

            migrationBuilder.DropTable(
                name: "TblBanner");

            migrationBuilder.DropIndex(
                name: "IX_TblProduct_SupplierCode",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "TblCartItem");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "TblCartItem");

            migrationBuilder.DropSequence(
                name: "banner_code_seq");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierCode",
                table: "TblProduct",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCodeNavigationCode",
                table: "TblProduct",
                type: "character varying",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblProduct_SupplierCodeNavigationCode",
                table: "TblProduct",
                column: "SupplierCodeNavigationCode");

            migrationBuilder.AddForeignKey(
                name: "FK_TblProduct_TblSupplier_SupplierCodeNavigationCode",
                table: "TblProduct",
                column: "SupplierCodeNavigationCode",
                principalTable: "TblSupplier",
                principalColumn: "Code");
        }
    }
}
