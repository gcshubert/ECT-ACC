using ECT.ACC.Data.Math;

namespace ECT.ACC.Data.Models;

/// <summary>
/// A named, ordered snapshot of which variant (or base) is active
/// for each parameter in a scenario.  Configurations are comparable —
/// Base (A) vs Proposed (B), or Initial Model (A) vs Observed (B).
/// </summary>
public class ScenarioConfiguration
{
    public int    Id          { get; set; }
    public int    ScenarioId  { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Display/narrative order within the scenario (0-based).
    /// Supports A → C1 → C2 → B sequencing for historical analyses.
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────────────────
    public Scenario                          Scenario { get; set; } = null!;
    public ICollection<ScenarioConfigurationEntry> Entries  { get; set; } = [];

    /// <summary>
    /// The deficit analysis produced when this configuration was last activated.
    /// Null until the configuration has been run at least once.
    /// </summary>
    public DeficitAnalysis? DeficitAnalysis { get; set; }
}

/// <summary>
/// One row per parameter in a configuration.
/// Records which variant was selected (null = Base chain)
/// and snapshots the composed value at activation time so
/// the result remains stable even if variant steps are later edited.
/// </summary>
public class ScenarioConfigurationEntry
{
    public int    Id              { get; set; }
    public int    ConfigurationId { get; set; }

    /// <summary>e.g. "energy", "control", "complexity", "timeAvailable"</summary>
    public string ParameterKey   { get; set; } = string.Empty;

    /// <summary>
    /// FK to ParameterVariant.  Null means the Base derivation chain
    /// was active for this parameter in this configuration.
    /// </summary>
    public int?   VariantId      { get; set; }

    /// <summary>
    /// Human-readable label snapshotted at activation time
    /// (e.g. "Base" or the variant name) so the record is self-describing.
    /// </summary>
    public string VariantLabel   { get; set; } = "Base";

    /// <summary>
    /// Composed value snapshotted at activation time.
    /// Protects the historical record if variant steps are later edited.
    /// </summary>
    public ScientificValueOwned? SnapshotValue { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────
    public ScenarioConfiguration Configuration { get; set; } = null!;
}
