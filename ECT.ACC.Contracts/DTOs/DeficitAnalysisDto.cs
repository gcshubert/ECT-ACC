namespace ECT.ACC.Contracts.DTOs;

public class DeficitAnalysisDto
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public ScientificValueDto CRequired { get; set; } = new();
    public ScientificValueDto CAvailable { get; set; } = new();
    public ScientificValueDto CDeficit { get; set; } = new();
    public string DeficitType { get; set; } = string.Empty;
    public string ClassificationNotes { get; set; } = string.Empty;
}
