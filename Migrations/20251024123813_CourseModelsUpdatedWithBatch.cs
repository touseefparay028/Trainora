using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class CourseModelsUpdatedWithBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_BatchId",
                table: "Courses",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_BatchDMs_BatchId",
                table: "Courses",
                column: "BatchId",
                principalTable: "BatchDMs",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_BatchDMs_BatchId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_BatchId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Courses");
        }
    }
}
