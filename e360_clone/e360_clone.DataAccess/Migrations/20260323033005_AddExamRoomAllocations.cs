using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class AddExamRoomAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamRoomAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    SeatNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRoomAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRoomAllocations_ExamRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "ExamRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRoomAllocations_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRoomAllocations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_MajorId",
                table: "Classes",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomAllocations_ExamId",
                table: "ExamRoomAllocations",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomAllocations_ExamId_StudentId",
                table: "ExamRoomAllocations",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomAllocations_RoomId",
                table: "ExamRoomAllocations",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomAllocations_RoomId_SeatNumber",
                table: "ExamRoomAllocations",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomAllocations_StudentId",
                table: "ExamRoomAllocations",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamRoomAllocations");

            migrationBuilder.DropIndex(
                name: "IX_Classes_MajorId",
                table: "Classes");
        }
    }
}
