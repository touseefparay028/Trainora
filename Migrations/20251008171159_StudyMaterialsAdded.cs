using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class StudyMaterialsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DueTime",
                table: "AssignmentDMs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudyMaterialsDMId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudyMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudyMaterialsDMId",
                table: "AspNetUsers",
                column: "StudyMaterialsDMId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StudyMaterials_StudyMaterialsDMId",
                table: "AspNetUsers",
                column: "StudyMaterialsDMId",
                principalTable: "StudyMaterials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StudyMaterials_StudyMaterialsDMId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "StudyMaterials");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StudyMaterialsDMId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StudyMaterialsDMId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueTime",
                table: "AssignmentDMs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
