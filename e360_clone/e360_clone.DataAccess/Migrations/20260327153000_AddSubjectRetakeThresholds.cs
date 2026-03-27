using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using e360_clone.DataAccess;

#nullable disable

namespace e360_clone.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260327153000_AddSubjectRetakeThresholds")]
    public partial class AddSubjectRetakeThresholds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinFinalScore",
                table: "Subjects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPracticalScore",
                table: "Subjects",
                type: "numeric",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinFinalScore",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "MinPracticalScore",
                table: "Subjects");
        }
    }
}
