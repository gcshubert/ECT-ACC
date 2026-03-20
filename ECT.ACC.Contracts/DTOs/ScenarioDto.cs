
namespace ECT.ACC.Contracts.DTOs;

public class ScenarioDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int? ProcessDomainId { get; set; }

    /// <summary>
    /// Data model mode: "Flat" or "Hierarchical".
    /// </summary>
    public string ScenarioMode { get; set; } = "Flat";

    /// <summary>
    /// Solve-for mode for the scenario.
    /// </summary>
    public string SolveForMode { get; set; } = "C";

    public ScenarioParametersDto? Parameters { get; set; }
    public DeficitAnalysisDto? DeficitAnalysis { get; set; }
}
