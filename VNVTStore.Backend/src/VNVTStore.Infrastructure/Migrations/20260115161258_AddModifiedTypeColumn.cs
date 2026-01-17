using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModifiedTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "TblUser",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'customer'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValueSql: "'customer'::character varying");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblUser",
                type: "text",
                nullable: true,
                defaultValue: "Add");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblSupplier",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblReview",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblReview",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblReview",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblQuote",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblQuote",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblQuote",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblProduct",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblProduct",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblProduct",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TblPayment",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'pending'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValueSql: "'pending'::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TblOrder",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'pending'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValueSql: "'pending'::character varying");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblOrder",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TblCoupon",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblCoupon",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCoupon",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblCategory",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblCategory",
                type: "text",
                nullable: true,
                defaultValue: "ADD");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TblCategory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblBanner",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedType",
                table: "TblBanner",
                type: "text",
                nullable: true,
                defaultValue: "ADD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblSupplier");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblReview");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblReview");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblReview");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblCoupon");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TblCoupon");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblCoupon");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblCoupon");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TblCategory");

            migrationBuilder.DropColumn(
                name: "ModifiedType",
                table: "TblBanner");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "TblUser",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                defaultValueSql: "'customer'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'customer'::character varying");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblQuote",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblProduct",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TblPayment",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                defaultValueSql: "'pending'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'pending'::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TblOrder",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                defaultValueSql: "'pending'::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'pending'::character varying");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TblCategory",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TblBanner",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
