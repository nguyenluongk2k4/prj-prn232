using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentConfirmToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StudentConfirmed",
                table: "Attendances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StudentConfirmedAt",
                table: "Attendances",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentConfirmed",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "StudentConfirmedAt",
                table: "Attendances");
        }
    }
}
