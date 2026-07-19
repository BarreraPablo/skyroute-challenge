using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyRoute.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialBookingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FlightId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    OriginCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DestinationCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsInternational = table.Column<bool>(type: "bit", nullable: false),
                    DepartureTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CabinClass = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PricePerPassenger = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PassengerCount = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passengers_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_BookingId",
                table: "Passengers",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Passengers");

            migrationBuilder.DropTable(
                name: "Bookings");
        }
    }
}
