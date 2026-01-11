using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Power",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "TblProduct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Voltage",
                table: "TblProduct",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Power",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "Voltage",
                table: "TblProduct");
        }
    }
}
