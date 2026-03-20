
namespace ECT.ACC.Data.Models;

public class Scenario
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Solve-for mode for the scenario (e.g. C, T, E, C_FromET, etc.).
    /// Used by V2 hierarchical analysis.
    /// </summary>
    public string SolveForMode { get; set; } = "C";

    /// <summary>
    /// Indicates whether the scenario uses the flat (V1) parameter model
    /// or the hierarchical (V2) graph model.
    /// </summary>
    public string ScenarioMode { get; set; } = "Flat";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // ── Existing navigation properties ────────────────────────────────────────
    public ScenarioParameters? Parameters { get; set; }

    /// <summary>
    /// All deficit analyses for this scenario (one per configuration).
    /// </summary>
    public ICollection<DeficitAnalysis> DeficitAnalyses { get; set; }
        = new List<DeficitAnalysis>();

    /// <summary>True if at least one configuration has been activated and run.</summary>
    public bool HasAnalysis => DeficitAnalyses.Any();

    // ── Phase 3 ───────────────────────────────────────────────────────────────
    public ICollection<ParameterDocumentation> ParameterDocumentations { get; set; }
        = new List<ParameterDocumentation>();

    // ── Phase 3.5 ─────────────────────────────────────────────────────────────
    public int? ProcessDomainId { get; set; }
    public ProcessDomain? ProcessDomain { get; set; }

    public ICollection<ParameterDefinition> ParameterDefinitions { get; set; }
        = new List<ParameterDefinition>();
}