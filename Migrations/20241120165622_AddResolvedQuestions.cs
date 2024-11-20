using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicStackOverflow.Migrations
{
    /// <inheritdoc />
    public partial class AddResolvedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Answered",
                table: "Posts",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BestAnswer",
                table: "Posts",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answered",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "BestAnswer",
                table: "Posts");
        }
    }
}
