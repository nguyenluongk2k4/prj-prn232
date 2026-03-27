using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""ExamRoomAllocations"" (""ExamId"", ""StudentId"", ""RoomId"", ""SeatNumber"", ""CreatedAt"")
                SELECT se.""ExamId"", se.""StudentId"", e.""RoomId"", 0, COALESCE(se.""CreatedAt"", NOW())
                FROM ""StudentExams"" se
                JOIN ""Exams"" e ON e.""Id"" = se.""ExamId""
                LEFT JOIN ""ExamRoomAllocations"" a
                    ON a.""ExamId"" = se.""ExamId"" AND a.""StudentId"" = se.""StudentId""
                WHERE a.""Id"" IS NULL;
            ");

            migrationBuilder.DropTable(
                name: "StudentExams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Grade = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentExams_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentExams_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_ExamId",
                table: "StudentExams",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_ExamId_StudentId",
                table: "StudentExams",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams",
                column: "StudentId");
        }
    }
}
