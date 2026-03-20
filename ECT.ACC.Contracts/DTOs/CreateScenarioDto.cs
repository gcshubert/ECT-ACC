namespace ECT.ACC.Contracts.DTOs;

public class CreateScenarioDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data model mode: "Flat" or "Hierarchical".
    /// </summary>
    public string ScenarioMode { get; set; } = "Flat";

    /// <summary>
    /// Solve-for mode for the scenario (C, T, E, C_FromET, etc.).
    /// </summary>
    public string SolveForMode { get; set; } = "C";

    public ScenarioParametersDto? Parameters { get; set; }
}
