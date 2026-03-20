using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECT.ACC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScenarioSolveForAndMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScenarioMode",
                table: "Scenarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Flat");

            migrationBuilder.AddColumn<string>(
                name: "SolveForMode",
                table: "Scenarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "C");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScenarioMode",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "SolveForMode",
                table: "Scenarios");
        }
    }
}
