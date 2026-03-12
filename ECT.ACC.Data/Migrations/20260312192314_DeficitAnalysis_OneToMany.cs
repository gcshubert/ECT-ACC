using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECT.ACC.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeficitAnalysis_OneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IX_DeficitAnalyses_ScenarioId (unique) was already absent from the DB;
            // just ensure the non-unique index exists for the FK.
            migrationBuilder.CreateIndex(
                name: "IX_DeficitAnalyses_ScenarioId",
                table: "DeficitAnalyses",
                column: "ScenarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeficitAnalyses_ScenarioId",
                table: "DeficitAnalyses");

            migrationBuilder.CreateIndex(
                name: "IX_DeficitAnalyses_ScenarioId",
                table: "DeficitAnalyses",
                column: "ScenarioId",
                unique: true);
        }
    }
}
