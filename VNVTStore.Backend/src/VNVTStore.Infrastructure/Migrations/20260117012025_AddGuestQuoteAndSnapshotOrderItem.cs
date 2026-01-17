using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestQuoteAndSnapshotOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "TblQuote_UserCode_fkey",
                table: "TblQuote");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblQuote",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "TblQuote",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "TblQuote",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "TblQuote",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "TblOrderItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "TblQuote_UserCode_fkey",
                table: "TblQuote",
                column: "UserCode",
                principalTable: "TblUser",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "TblQuote_UserCode_fkey",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "TblQuote");

            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "TblOrderItem");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "TblOrderItem");

            migrationBuilder.AlterColumn<string>(
                name: "UserCode",
                table: "TblQuote",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "TblQuote_UserCode_fkey",
                table: "TblQuote",
                column: "UserCode",
                principalTable: "TblUser",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
