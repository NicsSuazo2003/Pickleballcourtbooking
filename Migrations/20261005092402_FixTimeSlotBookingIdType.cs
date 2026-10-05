using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickleballBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixTimeSlotBookingIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "timeslots",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "timeslots",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "timeslots",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "timeslots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "timeslots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "timeslots");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "timeslots");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "timeslots",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "timeslots",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "timeslots",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
