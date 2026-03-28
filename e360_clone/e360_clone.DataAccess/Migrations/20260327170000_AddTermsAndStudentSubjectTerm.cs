using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e360_clone.Migrations
{
    public partial class AddTermsAndStudentSubjectTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Terms_Code",
                table: "Terms",
                column: "Code",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "TermId",
                table: "StudentSubjects",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Terms"" SET ""IsCurrent"" = false;
                WITH upsert AS (
                    INSERT INTO ""Terms"" (""Code"", ""Name"", ""StartDate"", ""EndDate"", ""IsCurrent"", ""CreatedAt"")
                    VALUES ('SPRING-2026', 'Spring 2026', '2026-01-01', '2026-05-31', true, NOW())
                    ON CONFLICT (""Code"") DO UPDATE
                        SET ""Name"" = EXCLUDED.""Name"",
                            ""StartDate"" = EXCLUDED.""StartDate"",
                            ""EndDate"" = EXCLUDED.""EndDate"",
                            ""IsCurrent"" = true
                    RETURNING ""Id""
                )
                UPDATE ""StudentSubjects"" SET ""TermId"" = (SELECT ""Id"" FROM upsert);
            ");

            migrationBuilder.AlterColumn<int>(
                name: "TermId",
                table: "StudentSubjects",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_Semester",
                table: "StudentSubjects");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_TermId",
                table: "StudentSubjects",
                columns: new[] { "StudentId", "SubjectId", "TermId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_TermId",
                table: "StudentSubjects",
                column: "TermId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSubjects_Terms_TermId",
                table: "StudentSubjects",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentSubjects_Terms_TermId",
                table: "StudentSubjects");

            migrationBuilder.DropIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_TermId",
                table: "StudentSubjects");

            migrationBuilder.DropIndex(
                name: "IX_StudentSubjects_TermId",
                table: "StudentSubjects");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_Semester",
                table: "StudentSubjects",
                columns: new[] { "StudentId", "SubjectId", "Semester" });

            migrationBuilder.DropColumn(
                name: "TermId",
                table: "StudentSubjects");

            migrationBuilder.DropTable(
                name: "Terms");
        }
    }
}
