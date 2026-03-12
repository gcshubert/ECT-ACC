namespace ECT.ACC.Contracts.DTOs;

// ── Response DTO ─────────────────────────────────────────────────────────────

public class ScenarioConfigurationDto
{
    public int    Id          { get; set; }
    public int    ScenarioId  { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    SortOrder   { get; set; }
    public string CreatedDate { get; set; } = string.Empty;

    public List<ScenarioConfigurationEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// The deficit result produced the last time this configuration was activated.
    /// Null if never run.
    /// </summary>
    public DeficitAnalysisDto? DeficitAnalysis { get; set; }
}

public class ScenarioConfigurationEntryDto
{
    public int    Id             { get; set; }
    public string ParameterKey  { get; set; } = string.Empty;
    public int?   VariantId     { get; set; }
    public string VariantLabel  { get; set; } = "Base";

    /// <summary>Composed value snapshotted at last activation. Null if never run.</summary>
    public ScientificValueDto? SnapshotValue { get; set; }
}

// ── Create ───────────────────────────────────────────────────────────────────

public class CreateScenarioConfigurationRequest
{
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// If provided, clone entries from this configuration id.
    /// If null, clone from current active state (Base + any active variants).
    /// </summary>
    public int? CloneFromConfigurationId { get; set; }
}

// ── Update (rename / redescribe / reorder) ───────────────────────────────────

public class UpdateScenarioConfigurationRequest
{
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    SortOrder   { get; set; }
}

// ── Update a single entry (swap variant for one parameter) ───────────────────

public class UpdateConfigurationEntryRequest
{
    /// <summary>Null = use Base chain for this parameter.</summary>
    public int? VariantId { get; set; }
}

// ── Activate (apply all entries + recompute deficit) ─────────────────────────

/// Request body is empty — the configuration id is in the route.
/// Response is the updated ScenarioConfigurationDto (with DeficitAnalysis populated).
