using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class AddMajorTableAndRelateClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_Classes_Subjects_MajorId'
                    ) THEN
                        ALTER TABLE ""Classes"" DROP CONSTRAINT ""FK_Classes_Subjects_MajorId"";
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MajorCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MajorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MajorGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Majors_MajorCode",
                table: "Majors",
                column: "MajorCode",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Majors"" (""Id"", ""MajorCode"", ""MajorName"", ""MajorGroup"", ""Status"", ""CreatedAt"")
                VALUES
                    (1, 'SE', 'Software Engineering', 'Technology', 'Active', NOW()),
                    (2, 'AI', 'Artificial Intelligence', 'Technology', 'Active', NOW()),
                    (3, 'IA', 'Information Assurance', 'Technology', 'Active', NOW()),
                    (4, 'SWD', 'Software Development', 'Technology', 'Active', NOW()),
                    (5, 'PRJ', 'Project', 'Technology', 'Active', NOW()),
                    (6, 'BUS', 'Business', 'Business', 'Active', NOW()),
                    (7, 'MKT', 'Marketing', 'Business', 'Active', NOW()),
                    (8, 'FIN', 'Finance', 'Business', 'Active', NOW()),
                    (9, 'ACC', 'Accounting', 'Business', 'Active', NOW()),
                    (10, 'EN', 'English', 'Language', 'Active', NOW()),
                    (11, 'JPN', 'Japanese', 'Language', 'Active', NOW()),
                    (12, 'KOR', 'Korean', 'Language', 'Active', NOW()),
                    (13, 'CHI', 'Chinese', 'Language', 'Active', NOW()),
                    (14, 'GD', 'Graphic Design', 'Design', 'Active', NOW()),
                    (15, 'MM', 'Multimedia Communication', 'Design', 'Active', NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Classes"" c
                SET ""MajorId"" = m.""Id""
                FROM ""Majors"" m
                WHERE c.""ClassCode"" LIKE m.""MajorCode"" || '%';
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Majors_MajorId",
                table: "Classes",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Majors_MajorId",
                table: "Classes");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Subjects_MajorId",
                table: "Classes",
                column: "MajorId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
