using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rentings");

            migrationBuilder.CreateTable(
                name: "Renting",
                schema: "rentings",
                columns: table => new
                {
                    leaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    leaseStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bookId = table.Column<Guid>(type: "uuid", nullable: false),
                    customerName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Renting", x => x.leaseId);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                schema: "rentings",
                columns: table => new
                {
                    reservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    bookId = table.Column<Guid>(type: "uuid", nullable: false),
                    reservedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customerName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.reservationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Renting",
                schema: "rentings");

            migrationBuilder.DropTable(
                name: "Reservations",
                schema: "rentings");
        }
    }
}
