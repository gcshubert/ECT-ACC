using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Math;

namespace ECT.ACC.Api.Services;

public interface IDeficitAnalysisService
{
    Task<DeficitAnalysisDto?> GetByScenarioIdAsync(int scenarioId);

    /// <summary>
    /// Legacy flat-parameter compute — used by the scenario-level deficit endpoint.
    /// Reads from ScenarioParameters (the flat E/C/k/T values).
    /// </summary>
    Task<DeficitAnalysisDto> ComputeAndSaveAsync(int scenarioId);

    /// <summary>
    /// Configuration-aware compute — used by ActivateConfigurationAsync.
    /// Accepts rollup-derived ScientificValues directly, bypassing the flat
    /// ScenarioParameters table. Result is tied to the given configurationId.
    /// </summary>
    Task<DeficitAnalysisDto> ComputeAndSaveFromRollupAsync(
        int scenarioId,
        int configurationId,
        ScientificValue energy,
        ScientificValue control,
        ScientificValue complexity,
        ScientificValue timeAvailable);
}
