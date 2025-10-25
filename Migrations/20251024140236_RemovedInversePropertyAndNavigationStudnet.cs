using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemovedInversePropertyAndNavigationStudnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_StudentsId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_StudentsId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StudentsId",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StudentsId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StudentsId",
                table: "Courses",
                column: "StudentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_StudentsId",
                table: "Courses",
                column: "StudentsId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
