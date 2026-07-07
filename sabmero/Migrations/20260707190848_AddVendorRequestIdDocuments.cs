using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorRequestIdDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenshipDocumentPath",
                table: "VendorRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NidDocumentPath",
                table: "VendorRequests",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$cE2VLq6XXIlVwKAul6IOPellsQCIO29BJPfEfUiZf8L9T2pgfRDHW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitizenshipDocumentPath",
                table: "VendorRequests");

            migrationBuilder.DropColumn(
                name: "NidDocumentPath",
                table: "VendorRequests");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ksXLyZOaIMYhzvwFGKtSzuq0DYgQkaf6mvjmtG4s0jRP/QX7EnYfK");
        }
    }
}
