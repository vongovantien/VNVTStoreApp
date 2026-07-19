using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VNVTStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "TblOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "TblOrder",
                type: "text",
                nullable: true);

            migrationBuilder.CreateSequence<int>(
                name: "delivery_code_seq",
                startValue: 1L);

            migrationBuilder.CreateTable(
                name: "TblDelivery",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "('DLV'::text || lpad((nextval('delivery_code_seq'::regclass))::text, 6, '0'::text))"),
                    OrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShipperCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShipperName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShipperPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'assigned'::character varying"),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedType = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblDelivery_pkey", x => x.Code);
                    table.ForeignKey(
                        name: "TblDelivery_OrderCode_fkey",
                        column: x => x.OrderCode,
                        principalTable: "TblOrder",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblDelivery_ShipperCode_fkey",
                        column: x => x.ShipperCode,
                        principalTable: "TblUser",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TblDeliveryHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeliveryCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedByCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TblDeliveryHistory_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "TblDeliveryHistory_DeliveryCode_fkey",
                        column: x => x.DeliveryCode,
                        principalTable: "TblDelivery",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "TblDeliveryHistory_UpdatedByCode_fkey",
                        column: x => x.UpdatedByCode,
                        principalTable: "TblUser",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "idx_delivery_order",
                table: "TblDelivery",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "idx_delivery_tracking",
                table: "TblDelivery",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TblDelivery_OrderCode",
                table: "TblDelivery",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblDelivery_ShipperCode",
                table: "TblDelivery",
                column: "ShipperCode");

            migrationBuilder.CreateIndex(
                name: "idx_deliveryhistory_delivery",
                table: "TblDeliveryHistory",
                column: "DeliveryCode");

            migrationBuilder.CreateIndex(
                name: "IX_TblDeliveryHistory_UpdatedByCode",
                table: "TblDeliveryHistory",
                column: "UpdatedByCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblDeliveryHistory");

            migrationBuilder.DropTable(
                name: "TblDelivery");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "TblOrder");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "TblOrder");

            migrationBuilder.DropSequence(
                name: "delivery_code_seq");
        }
    }
}
