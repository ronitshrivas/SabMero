using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorRequestsKycAndInstallationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KycRejectionReason",
                table: "Users",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KycStatus",
                table: "Users",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "NotSubmitted");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ServiceBookings",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<int>(
                name: "RelatedOrderId",
                table: "ServiceBookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentScreenshotPath",
                table: "Orders",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BusinessDocumentPath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Pending"),
                    RejectionReason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorRequests_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Key", "UpdatedAt", "Value" },
                values: new object[] { 1, "PaymentQrImagePath", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "/uploads/payment/admin-qr.png" });

            migrationBuilder.UpdateData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PaymentStatus", "RelatedOrderId" },
                values: new object[] { "Pending", null });

            migrationBuilder.UpdateData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PaymentStatus", "RelatedOrderId" },
                values: new object[] { "Submitted", null });

            migrationBuilder.UpdateData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PaymentStatus", "RelatedOrderId" },
                values: new object[] { "Pending", null });

            migrationBuilder.UpdateData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PaymentStatus", "RelatedOrderId" },
                values: new object[] { "Verified", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "KycRejectionReason", "KycStatus", "PasswordHash" },
                values: new object[] { null, "Approved", "$2a$11$ksXLyZOaIMYhzvwFGKtSzuq0DYgQkaf6mvjmtG4s0jRP/QX7EnYfK" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "KycRejectionReason", "KycStatus" },
                values: new object[] { null, "Approved" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "KycRejectionReason", "KycStatus" },
                values: new object[] { null, "Approved" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "KycRejectionReason", "KycStatus" },
                values: new object[] { null, "Approved" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "KycRejectionReason", "KycStatus" },
                values: new object[] { null, "Approved" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "KycRejectionReason", "KycStatus" },
                values: new object[] { null, "Approved" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_RelatedOrderId",
                table: "ServiceBookings",
                column: "RelatedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorRequests_UserId",
                table: "VendorRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorRequests_VendorId",
                table: "VendorRequests",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Orders_RelatedOrderId",
                table: "ServiceBookings",
                column: "RelatedOrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_Orders_RelatedOrderId",
                table: "ServiceBookings");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "VendorRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBookings_RelatedOrderId",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "KycRejectionReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KycStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "RelatedOrderId",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "PaymentScreenshotPath",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$HJ8n.AQyU2zH1HrFe0YaK.Lo80D.SII50md29A21a5EoLoXQTRjwq");
        }
    }
}
