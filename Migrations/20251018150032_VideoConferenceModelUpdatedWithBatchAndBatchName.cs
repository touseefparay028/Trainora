using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class VideoConferenceModelUpdatedWithBatchAndBatchName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VideoConference_BatchId",
                table: "VideoConference",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoConference_BatchDMs_BatchId",
                table: "VideoConference",
                column: "BatchId",
                principalTable: "BatchDMs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoConference_BatchDMs_BatchId",
                table: "VideoConference");

            migrationBuilder.DropIndex(
                name: "IX_VideoConference_BatchId",
                table: "VideoConference");
        }
    }
}
