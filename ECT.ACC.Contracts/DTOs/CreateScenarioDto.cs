namespace ECT.ACC.Contracts.DTOs;

public class CreateScenarioDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ScenarioParametersDto? Parameters { get; set; }
}
