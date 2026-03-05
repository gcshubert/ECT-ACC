namespace ECT.ACC.Contracts.DTOs;

public class UpdateScenarioParametersDto
{
    public ScientificValueDto Energy { get; set; } = new();
    public ScientificValueDto Control { get; set; } = new();
    public ScientificValueDto Complexity { get; set; } = new();
    public ScientificValueDto TimeAvailable { get; set; } = new();
}