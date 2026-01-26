using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
/*
            migrationBuilder.DropIndex(
                name: "idx_product_code",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Power",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Voltage",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "TblProduct");
*/

            migrationBuilder.AddColumn<string>(
                name: "BaseUnit",
                table: "TblProduct",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BinLocation",
                table: "TblProduct",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandCode",
                table: "TblProduct",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfOrigin",
                table: "TblProduct",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "TblProduct",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "TblProduct",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesalePrice",
                table: "TblProduct",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: true);

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
                name: "TblProductDetail",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DetailType = table.Column<string>(type: "text", nullable: false),
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
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblUnit_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblUnit_ProductCode_fkey",
                        column: x => x.ProductCode,
                        principalTable: "TblProduct",
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

            migrationBuilder.CreateIndex(
                name: "idx_product_brand",
                table: "TblProduct",
                column: "BrandCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductDetail_ProductCode",
                table: "TblProductDetail",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductTag_ProductCode",
                table: "TblProductTag",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblProductTag_TagCode",
                table: "TblProductTag",
                column: "TagCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblUnit_ProductCode",
                table: "TblUnit",
                column: "ProductCode");

            migrationBuilder.AddForeignKey(
                name: "FK_TblProduct_TblBrand_BrandCode",
                table: "TblProduct",
                column: "BrandCode",
                principalTable: "TblBrand",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblProduct_TblBrand_BrandCode",
                table: "TblProduct");

            migrationBuilder.DropTable(
                name: "TblBrand");

            migrationBuilder.DropTable(
                name: "TblProductDetail");

            migrationBuilder.DropTable(
                name: "TblProductTag");

            migrationBuilder.DropTable(
                name: "TblUnit");

            migrationBuilder.DropTable(
                name: "TblTag");

            migrationBuilder.DropIndex(
                name: "idx_product_brand",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "BaseUnit",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "BinLocation",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "BrandCode",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "CountryOfOrigin",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "TblProduct");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Power",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Voltage",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "TblProduct",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_product_code",
                table: "TblProduct",
                column: "Code");
        }
    }
}
