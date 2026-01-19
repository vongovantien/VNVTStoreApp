using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterCodeToFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "MasterCode",
                table: "TblFile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MasterType",
                table: "TblFile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_file_mastercode",
                table: "TblFile",
                column: "MasterCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_file_mastercode",
                table: "TblFile");

            migrationBuilder.DropColumn(
                name: "MasterCode",
                table: "TblFile");

            migrationBuilder.DropColumn(
                name: "MasterType",
                table: "TblFile");

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
    }
}
