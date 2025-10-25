using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ForiegnKeyStudentInAttendances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentId1",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "AnnouncementsVM");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId1",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "Attendances");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentId",
                table: "Attendances",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnnouncementsVM",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementsVM", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId1",
                table: "Attendances",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentId1",
                table: "Attendances",
                column: "StudentId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
