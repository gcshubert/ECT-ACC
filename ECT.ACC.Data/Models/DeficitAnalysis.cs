using ECT.ACC.Data.Math;

namespace ECT.ACC.Data.Models;

public class DeficitAnalysis
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public ScientificValue CRequired { get; set; }
    public ScientificValue CAvailable { get; set; }
    public ScientificValue CDeficit { get; set; }
    public string DeficitType { get; set; } = string.Empty;
    public string ClassificationNotes { get; set; } = string.Empty;

    /// <summary>
    /// FK to the ScenarioConfiguration that produced this result.
    /// Null for ad-hoc runs (e.g. from the Parameters tab).
    /// </summary>
    public int? ConfigurationId { get; set; }
    public ScenarioConfiguration? Configuration { get; set; }

    // Navigation property
    public Scenario? Scenario { get; set; }


}
