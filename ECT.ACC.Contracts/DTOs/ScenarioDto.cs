
namespace ECT.ACC.Contracts.DTOs;

public class ScenarioDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public ScenarioParametersDto? Parameters { get; set; }
    public DeficitAnalysisDto? DeficitAnalysis { get; set; }
}
