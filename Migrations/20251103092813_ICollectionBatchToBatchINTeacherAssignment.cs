using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ICollectionBatchToBatchINTeacherAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchDMTeacherAssignmentDM");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDMs_BatchDMId",
                table: "AssignmentDMs",
                column: "BatchDMId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentDMs_BatchDMs_BatchDMId",
                table: "AssignmentDMs",
                column: "BatchDMId",
                principalTable: "BatchDMs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentDMs_BatchDMs_BatchDMId",
                table: "AssignmentDMs");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentDMs_BatchDMId",
                table: "AssignmentDMs");

            migrationBuilder.CreateTable(
                name: "BatchDMTeacherAssignmentDM",
                columns: table => new
                {
                    BatchDMid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherAssignmentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchDMTeacherAssignmentDM", x => new { x.BatchDMid, x.TeacherAssignmentsId });
                    table.ForeignKey(
                        name: "FK_BatchDMTeacherAssignmentDM_AssignmentDMs_TeacherAssignmentsId",
                        column: x => x.TeacherAssignmentsId,
                        principalTable: "AssignmentDMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchDMTeacherAssignmentDM_BatchDMs_BatchDMid",
                        column: x => x.BatchDMid,
                        principalTable: "BatchDMs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchDMTeacherAssignmentDM_TeacherAssignmentsId",
                table: "BatchDMTeacherAssignmentDM",
                column: "TeacherAssignmentsId");
        }
    }
}
