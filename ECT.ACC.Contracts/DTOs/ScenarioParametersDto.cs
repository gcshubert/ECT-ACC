namespace ECT.ACC.Contracts.DTOs;

public class ScenarioParametersDto
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public ScientificValueDto Energy { get; set; } = new();
    public ScientificValueDto Control { get; set; } = new();
    public ScientificValueDto Complexity { get; set; } = new();
    public ScientificValueDto TimeAvailable { get; set; } = new();
}
