using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceProductImageWithFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TblProductCode",
                table: "TblFile",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblFile_TblProductCode",
                table: "TblFile",
                column: "TblProductCode");

            migrationBuilder.AddForeignKey(
                name: "FK_TblFile_TblProduct_TblProductCode",
                table: "TblFile",
                column: "TblProductCode",
                principalTable: "TblProduct",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblFile_TblProduct_TblProductCode",
                table: "TblFile");

            migrationBuilder.DropIndex(
                name: "IX_TblFile_TblProductCode",
                table: "TblFile");

            migrationBuilder.DropColumn(
                name: "TblProductCode",
                table: "TblFile");
        }
    }
}
