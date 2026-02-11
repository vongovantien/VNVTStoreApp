using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReviewSelfReferencing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Defensive cleanup to handle potential existing indices/constraints from interrupted or inconsistent migrations
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_review_product;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_TblReview_ProductCode\";");
            migrationBuilder.Sql("ALTER TABLE \"TblReview\" DROP CONSTRAINT IF EXISTS \"TblReview_ProductCode_fkey\";");
            migrationBuilder.Sql("ALTER TABLE \"TblReview\" DROP CONSTRAINT IF EXISTS \"TblReview_ParentCode_fkey\";");

            migrationBuilder.DropColumn(
                name: "AdminReply",
                table: "TblReview");

            migrationBuilder.AddColumn<string>(
                name: "ParentCode",
                table: "TblReview",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_ParentCode",
                table: "TblReview",
                column: "ParentCode");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblReview",
                type: "character varying(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblReview_ProductCode",
                table: "TblReview",
                column: "ProductCode");

            migrationBuilder.AddForeignKey(
                name: "TblReview_ParentCode_fkey",
                table: "TblReview",
                column: "ParentCode",
                principalTable: "TblReview",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "TblReview_ProductCode_fkey",
                table: "TblReview",
                column: "ProductCode",
                principalTable: "TblProduct",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "TblReview_ParentCode_fkey",
                table: "TblReview");

            migrationBuilder.DropForeignKey(
                name: "TblReview_ProductCode_fkey",
                table: "TblReview");

            migrationBuilder.DropIndex(
                name: "IX_TblReview_ProductCode",
                table: "TblReview");

            migrationBuilder.DropIndex(
                name: "IX_TblReview_ParentCode",
                table: "TblReview");

            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "TblReview");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "TblReview",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminReply",
                table: "TblReview",
                type: "text",
                nullable: true);
        }
    }
}
