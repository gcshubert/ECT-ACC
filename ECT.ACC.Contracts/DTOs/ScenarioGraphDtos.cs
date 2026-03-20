namespace ECT.ACC.Contracts.DTOs;

public class ScenarioNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SolveForMode { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ExternalScenarioId { get; set; } = string.Empty;
}
