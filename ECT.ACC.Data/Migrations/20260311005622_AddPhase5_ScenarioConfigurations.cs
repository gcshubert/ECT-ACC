using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECT.ACC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase5_ScenarioConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfigurationId",
                table: "DeficitAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScenarioConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioConfigurations_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioConfigurationEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ParameterKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    VariantLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "Base"),
                    SnapshotCoefficient = table.Column<double>(type: "float", nullable: true),
                    SnapshotExponent = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioConfigurationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioConfigurationEntries_ScenarioConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "ScenarioConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeficitAnalyses_ConfigurationId",
                table: "DeficitAnalyses",
                column: "ConfigurationId",
                unique: true,
                filter: "[ConfigurationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioConfigurationEntries_ConfigurationId_ParameterKey",
                table: "ScenarioConfigurationEntries",
                columns: new[] { "ConfigurationId", "ParameterKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioConfigurations_ScenarioId",
                table: "ScenarioConfigurations",
                column: "ScenarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeficitAnalyses_ScenarioConfigurations_ConfigurationId",
                table: "DeficitAnalyses",
                column: "ConfigurationId",
                principalTable: "ScenarioConfigurations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeficitAnalyses_ScenarioConfigurations_ConfigurationId",
                table: "DeficitAnalyses");

            migrationBuilder.DropTable(
                name: "ScenarioConfigurationEntries");

            migrationBuilder.DropTable(
                name: "ScenarioConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_DeficitAnalyses_ConfigurationId",
                table: "DeficitAnalyses");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "DeficitAnalyses");
        }
    }
}
