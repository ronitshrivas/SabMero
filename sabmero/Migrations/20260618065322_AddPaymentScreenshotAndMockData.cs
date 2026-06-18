using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentScreenshotAndMockData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentScreenshotPath",
                table: "ServiceBookings",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$HJ8n.AQyU2zH1HrFe0YaK.Lo80D.SII50md29A21a5EoLoXQTRjwq");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FullName", "IsActive", "IsKycVerified", "KycDocumentPath", "PasswordHash", "Phone", "Role" },
                values: new object[,]
                {
                    { 2, "Baneshwor, Kathmandu", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "ramesh@example.com", "Ramesh Thapa", true, true, null, "$2b$11$J7WSywJFE37tTzHqVU321eYep8c4Muw0ewXAgQelgLn/Q.y3iUiZm", "9811111111", "Customer" },
                    { 3, "Lakeside, Pokhara", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "sita@example.com", "Sita Gurung", true, true, null, "$2b$11$J7WSywJFE37tTzHqVU321eYep8c4Muw0ewXAgQelgLn/Q.y3iUiZm", "9822222222", "Customer" },
                    { 4, "Kalanki, Kathmandu", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "bibek.tech@example.com", "Bibek Shrestha", true, true, null, "$2b$11$J7WSywJFE37tTzHqVU321eYep8c4Muw0ewXAgQelgLn/Q.y3iUiZm", "9833333333", "Technician" },
                    { 5, "Lalitpur, Kathmandu", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "kiran.tech@example.com", "Kiran Magar", true, true, null, "$2b$11$J7WSywJFE37tTzHqVU321eYep8c4Muw0ewXAgQelgLn/Q.y3iUiZm", "9844444444", "Technician" },
                    { 6, "New Road, Kathmandu", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "hari.vendor@example.com", "Hari Bahadur", true, true, null, "$2b$11$J7WSywJFE37tTzHqVU321eYep8c4Muw0ewXAgQelgLn/Q.y3iUiZm", "9855555555", "Vendor" }
                });

            migrationBuilder.InsertData(
                table: "ServiceBookings",
                columns: new[] { "Id", "BookingDate", "CheckInTime", "CompletedTime", "CreatedAt", "DamageImagePath", "Latitude", "Longitude", "PaymentMethod", "PaymentScreenshotPath", "ServiceAddress", "ServiceCharge", "ServiceType", "Status", "TechnicianId", "TimeSlot", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 27.717199999999998, 85.323999999999998, "Cash", null, "Baneshwor, Kathmandu", null, "Electrical", "Pending", null, "10:00 AM - 12:00 PM", 2 },
                    { 2, new DateTime(2026, 6, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 28.209599999999998, 83.985600000000005, "QR", "/uploads/payment/mock-qr-payment-1.jpg", "Lakeside, Pokhara", null, "CCTV", "Pending", null, "02:00 PM - 04:00 PM", 3 },
                    { 3, new DateTime(2026, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 27.717199999999998, 85.323999999999998, "Cash", null, "Baneshwor, Kathmandu", null, "Tech", "Approved", 4, "09:00 AM - 11:00 AM", 2 },
                    { 4, new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 15, 7, 15, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 15, 9, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 28.209599999999998, 83.985600000000005, "QR", "/uploads/payment/mock-qr-payment-2.jpg", "Lakeside, Pokhara", 1500m, "Electrical", "Completed", 5, "01:00 PM - 03:00 PM", 3 }
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "Id", "BusinessAddress", "BusinessDocumentPath", "BusinessName", "CommissionRate", "CreatedAt", "IsApproved", "UserId" },
                values: new object[] { 1, "New Road, Kathmandu", null, "Hari Electronics Store", 10.0m, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), true, 6 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "ColorOptions", "CreatedAt", "Description", "ImagePath", "IsActive", "Name", "Price", "SizeOptions", "Stock", "Unit", "VendorId" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Bluetooth 5.3 noise-cancelling earbuds.", null, true, "Wireless Earbuds", 2499m, null, 50, null, 1 },
                    { 2, 1, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Wi-Fi controlled 9W colour-changing bulb.", null, true, "Smart LED Bulb", 899m, null, 120, null, 1 },
                    { 3, 2, "[\"Black\",\"White\",\"Navy\"]", new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Unisex round-neck cotton t-shirt.", null, true, "Cotton T-Shirt", 699m, "[\"S\",\"M\",\"L\",\"XL\"]", 200, null, 1 },
                    { 4, 4, null, new DateTime(2026, 6, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Premium aged basmati rice.", null, true, "Basmati Rice", 180m, null, 300, "Kg", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ServiceBookings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vendors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "PaymentScreenshotPath",
                table: "ServiceBookings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rQarYeyOZOiWAq.yLO/NfO0v1Z4SMD4/PhLAaTkEr07q92mmrqJfm");
        }
    }
}
