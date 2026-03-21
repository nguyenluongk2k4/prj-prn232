using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExamSchedule_SimplifyExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_ExamSchedules_ExamScheduleId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_ProctorAssignments_ExamSchedules_ExamScheduleId",
                table: "ProctorAssignments");

            migrationBuilder.DropTable(
                name: "ExamSchedules");

            migrationBuilder.RenameColumn(
                name: "ExamScheduleId",
                table: "ProctorAssignments",
                newName: "ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_ProctorAssignments_ExamScheduleId",
                table: "ProctorAssignments",
                newName: "IX_ProctorAssignments_ExamId");

            migrationBuilder.RenameColumn(
                name: "ExamScheduleId",
                table: "Attendances",
                newName: "ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_ExamScheduleId",
                table: "Attendances",
                newName: "IX_Attendances_ExamId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Exams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Exams",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CreatedBy",
                table: "Exams",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Exams_ExamId",
                table: "Attendances",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Accounts_CreatedBy",
                table: "Exams",
                column: "CreatedBy",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProctorAssignments_Exams_ExamId",
                table: "ProctorAssignments",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Exams_ExamId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Accounts_CreatedBy",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProctorAssignments_Exams_ExamId",
                table: "ProctorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CreatedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "ProctorAssignments",
                newName: "ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ProctorAssignments_ExamId",
                table: "ProctorAssignments",
                newName: "IX_ProctorAssignments_ExamScheduleId");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "Attendances",
                newName: "ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_ExamId",
                table: "Attendances",
                newName: "IX_Attendances_ExamScheduleId");

            migrationBuilder.CreateTable(
                name: "ExamSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProctorIds = table.Column<List<int>>(type: "integer[]", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Scheduled"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_ExamRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "ExamRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_ExamId",
                table: "ExamSchedules",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_RoomId",
                table: "ExamSchedules",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_ExamSchedules_ExamScheduleId",
                table: "Attendances",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProctorAssignments_ExamSchedules_ExamScheduleId",
                table: "ProctorAssignments",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
