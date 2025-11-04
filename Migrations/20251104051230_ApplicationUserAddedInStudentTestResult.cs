using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserAddedInStudentTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StudentTestResults_StudentId",
                table: "StudentTestResults",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTestResults_AspNetUsers_StudentId",
                table: "StudentTestResults",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentTestResults_AspNetUsers_StudentId",
                table: "StudentTestResults");

            migrationBuilder.DropIndex(
                name: "IX_StudentTestResults_StudentId",
                table: "StudentTestResults");
        }
    }
}
