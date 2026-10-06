using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace sabmero.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepairServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Charge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairServices", x => x.Id);
                });

            // Seed the default services (no Id → identity auto-assigns & advances the sequence).
            migrationBuilder.InsertData(
                table: "RepairServices",
                columns: new[] { "Name", "Description", "Charge", "ImagePath", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { "Electrical Repair", "Wiring, switches, sockets, fans, lights, circuit breakers, and all electrical work.", 500m, null, true, new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
                    { "CCTV Services", "Installation, maintenance, and repair of CCTV cameras and security systems.", 800m, null, true, new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
                    { "Tech Repair", "Computer, laptop, mobile phone, printer, and smart TV repair services.", 600m, null, true, new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
                    { "Plumbing", "Pipe repair, tap replacement, water tank installation, and drain cleaning.", 400m, null, true, new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairServices");
        }
    }
}
