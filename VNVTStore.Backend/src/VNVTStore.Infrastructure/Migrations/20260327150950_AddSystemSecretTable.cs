using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSecretTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblPermission");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblPermission");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblPermission");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "TblProduct",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "TblProduct",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TblSystemSecret",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecretValue = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedType = table.Column<string>(type: "text", nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblSystemSecret_pkey", x => x.Code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblSystemSecret");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "TblProduct");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblPermission",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblPermission",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblPermission",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
