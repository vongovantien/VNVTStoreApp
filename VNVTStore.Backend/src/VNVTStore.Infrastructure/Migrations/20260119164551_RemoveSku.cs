using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure constraint is dropped first if it exists (Index backing it cannot be dropped otherwise)
            migrationBuilder.Sql("ALTER TABLE \"TblProduct\" DROP CONSTRAINT IF EXISTS \"TblProduct_SKU_key\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"TblProduct_SKU_key\";");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "TblProduct");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "TblProduct",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "TblProduct_SKU_key",
                table: "TblProduct",
                column: "SKU",
                unique: true);
        }
    }
}
