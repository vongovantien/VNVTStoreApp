using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "auditlog_code_seq");

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "TblProduct",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "TblProduct",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TblAuditLog",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('LOG'::text || lpad((nextval('auditlog_code_seq'::regclass))::text, 6, '0'::text))"),
                    UserCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Detail = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "ADD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblAuditLog_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblAuditLog_UserCode_fkey",
                        column: x => x.UserCode,
                        principalTable: "TblUser",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblAuditLog_UserCode",
                table: "TblAuditLog",
                column: "UserCode");

            // Initialize Rating and ReviewCount data
            migrationBuilder.Sql(@"
                UPDATE ""TblProduct"" p
                SET 
                    ""Rating"" = COALESCE(sub.AvgRating, 0),
                    ""ReviewCount"" = COALESCE(sub.ReviewCnt, 0)
                FROM (
                    SELECT 
                        ""ProductCode"", 
                        AVG(""Rating"")::DECIMAL(3,2) as AvgRating, 
                        COUNT(*) as ReviewCnt
                    FROM ""TblReview""
                    WHERE ""IsApproved"" = true AND ""IsActive"" = true
                    GROUP BY ""ProductCode""
                ) sub
                WHERE p.""Code"" = sub.""ProductCode"";
            ");

            migrationBuilder.Sql(@"
                UPDATE ""TblProduct"" p
                SET 
                    ""Rating"" = COALESCE(sub.AvgRating, 0),
                    ""ReviewCount"" = p.""ReviewCount"" + COALESCE(sub.ReviewCnt, 0)
                FROM (
                    SELECT 
                        oi.""ProductCode"", 
                        AVG(r.""Rating"")::DECIMAL(3,2) as AvgRating, 
                        COUNT(*) as ReviewCnt
                    FROM ""TblReview"" r
                    JOIN ""TblOrderItem"" oi ON r.""OrderItemCode"" = oi.""Code""
                    WHERE r.""IsApproved"" = true AND r.""IsActive"" = true AND r.""ProductCode"" IS NULL
                    GROUP BY oi.""ProductCode""
                ) sub
                WHERE p.""Code"" = sub.""ProductCode"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblAuditLog");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "TblProduct");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "TblProduct");

            migrationBuilder.DropSequence(
                name: "auditlog_code_seq");
        }
    }
}
