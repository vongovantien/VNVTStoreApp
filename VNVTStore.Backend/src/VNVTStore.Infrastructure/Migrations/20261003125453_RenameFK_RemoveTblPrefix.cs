using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFK_RemoveTblPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TblQuote.TblProductCode is a leftover column from an old schema design
            // where a quote had a single product. The entity was refactored to use
            // TblQuoteItems (one-to-many) instead. This column is no longer mapped
            // in the domain model and can be safely dropped.
            migrationBuilder.DropColumn(
                name: "TblProductCode",
                table: "TblQuote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the orphan column if rolling back
            migrationBuilder.AddColumn<string>(
                name: "TblProductCode",
                table: "TblQuote",
                type: "varchar(100)",
                nullable: true);
        }
    }
}
