using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCodeToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "TblReview",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_review_product",
                table: "TblReview",
                column: "ProductCode");

            migrationBuilder.AddForeignKey(
                name: "TblReview_ProductCode_fkey",
                table: "TblReview",
                column: "ProductCode",
                principalTable: "TblProduct",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "TblReview_ProductCode_fkey",
                table: "TblReview");

            migrationBuilder.DropIndex(
                name: "idx_review_product",
                table: "TblReview");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "TblReview");
        }
    }
}
