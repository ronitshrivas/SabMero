using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentQrPath",
                table: "Vendors",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VendorPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ScreenshotPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Paid"),
                    PaidByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorPayments_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorPayments_VendorId",
                table: "VendorPayments",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "PaymentQrPath",
                table: "Vendors");
        }
    }
}
