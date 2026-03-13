using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTierColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserTier",
                table: "TblUser",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "TblProduct",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserTier",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "TblProduct");
        }
    }
}
