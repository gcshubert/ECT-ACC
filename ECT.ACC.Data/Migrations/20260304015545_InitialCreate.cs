using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECT.ACC.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeficitAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    CRequiredCoefficient = table.Column<double>(type: "float", nullable: false),
                    CRequiredExponent = table.Column<double>(type: "float", nullable: false),
                    CAvailableCoefficient = table.Column<double>(type: "float", nullable: false),
                    CAvailableExponent = table.Column<double>(type: "float", nullable: false),
                    CDeficitCoefficient = table.Column<double>(type: "float", nullable: false),
                    CDeficitExponent = table.Column<double>(type: "float", nullable: false),
                    DeficitType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClassificationNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeficitAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeficitAnalyses_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    EnergyCoefficient = table.Column<double>(type: "float", nullable: false),
                    EnergyExponent = table.Column<double>(type: "float", nullable: false),
                    ControlCoefficient = table.Column<double>(type: "float", nullable: false),
                    ControlExponent = table.Column<double>(type: "float", nullable: false),
                    ComplexityCoefficient = table.Column<double>(type: "float", nullable: false),
                    ComplexityExponent = table.Column<double>(type: "float", nullable: false),
                    TimeAvailableCoefficient = table.Column<double>(type: "float", nullable: false),
                    TimeAvailableExponent = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioParameters_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeficitAnalyses_ScenarioId",
                table: "DeficitAnalyses",
                column: "ScenarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioParameters_ScenarioId",
                table: "ScenarioParameters",
                column: "ScenarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeficitAnalyses");

            migrationBuilder.DropTable(
                name: "ScenarioParameters");

            migrationBuilder.DropTable(
                name: "Scenarios");
        }
    }
}
