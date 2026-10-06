using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDescriptionAndDamagePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DamageImagePathsCsv",
                table: "ServiceBookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ServiceBookings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageImagePathsCsv",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ServiceBookings");
        }
    }
}
