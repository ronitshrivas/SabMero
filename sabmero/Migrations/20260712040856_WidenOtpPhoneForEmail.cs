using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class WidenOtpPhoneForEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "OtpCodes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Cy5vxd/1Z1aQpw58Gs0iMe0Kmpi8v/Q5S/IrhEUcUCTLEdfK6NQUu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "OtpCodes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nQTKxjX05B6whTdV5aE6GePiDWq8H9HRrOwLX6dox8jVO1RfpB/OO");
        }
    }
}
