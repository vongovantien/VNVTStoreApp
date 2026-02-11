using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenusAndRoleMenusTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
// migrationBuilder.DropColumn(
            //     name: "AccessChannel",
            //     table: "TblRole");

            migrationBuilder.CreateTable(
                name: "TblMenu",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GroupCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblMenu_pkey", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblRoleMenu",
                columns: table => new
                {
                    RoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MenuCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblRoleMenu_pkey", x => new { x.RoleCode, x.MenuCode });
                    table.ForeignKey(
                        name: "TblRoleMenu_MenuCode_fkey",
                        column: x => x.MenuCode,
                        principalTable: "TblMenu",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblRoleMenu_RoleCode_fkey",
                        column: x => x.RoleCode,
                        principalTable: "TblRole",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblRoleMenu_MenuCode",
                table: "TblRoleMenu",
                column: "MenuCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblRoleMenu");

            migrationBuilder.DropTable(
                name: "TblMenu");

// migrationBuilder.AddColumn<string>(
            //     name: "AccessChannel",
            //     table: "TblRole",
            //     type: "text",
            //     nullable: true);
        }
    }
}
