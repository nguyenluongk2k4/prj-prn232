using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCohortColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cohort",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CohortYear",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""Classes""
                SET ""Cohort"" = CAST(substring(""ClassCode"" from '[0-9]{2}') AS integer)
                WHERE ""ClassCode"" ~ '[0-9]{2}';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Classes""
                SET ""CohortYear"" = 2000 + ""Cohort""
                WHERE ""Cohort"" > 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cohort",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CohortYear",
                table: "Classes");
        }
    }
}
