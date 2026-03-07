
namespace ECT.ACC.Data.Models;

public class Scenario
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // ── Existing navigation properties ────────────────────────────────────────
    public ScenarioParameters? Parameters { get; set; }
    public DeficitAnalysis? DeficitAnalysis { get; set; }

    // ── Phase 3 ───────────────────────────────────────────────────────────────
    public ICollection<ParameterDocumentation> ParameterDocumentations { get; set; }
        = new List<ParameterDocumentation>();

    // ── Phase 3.5 ─────────────────────────────────────────────────────────────
    public int? ProcessDomainId { get; set; }
    public ProcessDomain? ProcessDomain { get; set; }

    public ICollection<ParameterDefinition> ParameterDefinitions { get; set; }
        = new List<ParameterDefinition>();
}