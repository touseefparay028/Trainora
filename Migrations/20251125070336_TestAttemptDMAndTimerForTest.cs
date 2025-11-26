using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class TestAttemptDMAndTimerForTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TestAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_TestId",
                table: "TestAttempts",
                column: "TestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Tests");
        }
    }
}
