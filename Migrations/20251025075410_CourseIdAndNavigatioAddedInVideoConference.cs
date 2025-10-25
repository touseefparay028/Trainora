using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class CourseIdAndNavigatioAddedInVideoConference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "VideoConference",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VideoConference_CourseId",
                table: "VideoConference",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoConference_Courses_CourseId",
                table: "VideoConference",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoConference_Courses_CourseId",
                table: "VideoConference");

            migrationBuilder.DropIndex(
                name: "IX_VideoConference_CourseId",
                table: "VideoConference");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "VideoConference");
        }
    }
}
