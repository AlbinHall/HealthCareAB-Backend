using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCareABApi.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Feedback",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Feedback");
        }
    }
}
