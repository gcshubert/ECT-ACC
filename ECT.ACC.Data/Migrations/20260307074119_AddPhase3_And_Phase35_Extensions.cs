using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECT.ACC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3_And_Phase35_Extensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessDomainId",
                table: "Scenarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEctCoreParameter = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue_Coefficient = table.Column<double>(type: "float", nullable: true),
                    DefaultValue_Exponent = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDefinitions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterDocumentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    ParameterKey = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DerivationNarrative = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDocumentations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDocumentations_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IconKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterDocumentationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterVariants_ParameterDocumentations_ParameterDocumentationId",
                        column: x => x.ParameterDocumentationId,
                        principalTable: "ParameterDocumentations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterDocumentationId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value_Coefficient = table.Column<double>(type: "float", nullable: false),
                    Value_Exponent = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubParameters_ParameterDocumentations_ParameterDocumentationId",
                        column: x => x.ParameterDocumentationId,
                        principalTable: "ParameterDocumentations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessDomainId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterTemplates_ProcessDomains_ProcessDomainId",
                        column: x => x.ProcessDomainId,
                        principalTable: "ProcessDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantSubParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterVariantId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value_Coefficient = table.Column<double>(type: "float", nullable: false),
                    Value_Exponent = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantSubParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantSubParameters_ParameterVariants_ParameterVariantId",
                        column: x => x.ParameterVariantId,
                        principalTable: "ParameterVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterTemplateId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DefaultUnit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEctCoreParameter = table.Column<bool>(type: "bit", nullable: false),
                    SeedValue_Coefficient = table.Column<double>(type: "float", nullable: true),
                    SeedValue_Exponent = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateParameterDefinitions_ParameterTemplates_ParameterTemplateId",
                        column: x => x.ParameterTemplateId,
                        principalTable: "ParameterTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ProcessDomainId",
                table: "Scenarios",
                column: "ProcessDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_ScenarioId_Key",
                table: "ParameterDefinitions",
                columns: new[] { "ScenarioId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDocumentations_ScenarioId_ParameterKey",
                table: "ParameterDocumentations",
                columns: new[] { "ScenarioId", "ParameterKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterTemplates_ProcessDomainId",
                table: "ParameterTemplates",
                column: "ProcessDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterVariants_ParameterDocumentationId",
                table: "ParameterVariants",
                column: "ParameterDocumentationId");

            migrationBuilder.CreateIndex(
                name: "IX_SubParameters_ParameterDocumentationId",
                table: "SubParameters",
                column: "ParameterDocumentationId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateParameterDefinitions_ParameterTemplateId",
                table: "TemplateParameterDefinitions",
                column: "ParameterTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantSubParameters_ParameterVariantId",
                table: "VariantSubParameters",
                column: "ParameterVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scenarios_ProcessDomains_ProcessDomainId",
                table: "Scenarios",
                column: "ProcessDomainId",
                principalTable: "ProcessDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scenarios_ProcessDomains_ProcessDomainId",
                table: "Scenarios");

            migrationBuilder.DropTable(
                name: "ParameterDefinitions");

            migrationBuilder.DropTable(
                name: "SubParameters");

            migrationBuilder.DropTable(
                name: "TemplateParameterDefinitions");

            migrationBuilder.DropTable(
                name: "VariantSubParameters");

            migrationBuilder.DropTable(
                name: "ParameterTemplates");

            migrationBuilder.DropTable(
                name: "ParameterVariants");

            migrationBuilder.DropTable(
                name: "ProcessDomains");

            migrationBuilder.DropTable(
                name: "ParameterDocumentations");

            migrationBuilder.DropIndex(
                name: "IX_Scenarios_ProcessDomainId",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "ProcessDomainId",
                table: "Scenarios");
        }
    }
}
