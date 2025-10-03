using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class NewVC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VideoConferenceId",
                table: "BatchDMs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchDMs_VideoConferenceId",
                table: "BatchDMs",
                column: "VideoConferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchDMs_VideoConference_VideoConferenceId",
                table: "BatchDMs",
                column: "VideoConferenceId",
                principalTable: "VideoConference",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchDMs_VideoConference_VideoConferenceId",
                table: "BatchDMs");

            migrationBuilder.DropIndex(
                name: "IX_BatchDMs_VideoConferenceId",
                table: "BatchDMs");

            migrationBuilder.DropColumn(
                name: "VideoConferenceId",
                table: "BatchDMs");
        }
    }
}
