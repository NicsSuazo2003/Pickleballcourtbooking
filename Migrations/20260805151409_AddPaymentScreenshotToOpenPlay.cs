using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickleballBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentScreenshotToOpenPlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentScreenshot",
                table: "openplayregistrations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentScreenshot",
                table: "openplayregistrations");
        }
    }
}
