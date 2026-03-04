using ECT.ACC.Contracts.DTOs;
namespace ECT.ACC.Api.Services;

public interface IDeficitAnalysisService
{
    Task<DeficitAnalysisDto?> GetByScenarioIdAsync(int  scenarioId);
    Task<DeficitAnalysisDto> ComputeAndSaveAsync(int scenarioId);
}
