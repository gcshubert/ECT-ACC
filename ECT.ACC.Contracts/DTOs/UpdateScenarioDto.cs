
namespace ECT.ACC.Contracts.DTOs;

public class UpdateScenarioDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public string? ScenarioMode { get; set; }
    public string? SolveForMode { get; set; }
}
