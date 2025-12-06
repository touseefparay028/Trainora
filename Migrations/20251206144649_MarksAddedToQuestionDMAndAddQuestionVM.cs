using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class MarksAddedToQuestionDMAndAddQuestionVM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Marks",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marks",
                table: "Questions");
        }
    }
}
