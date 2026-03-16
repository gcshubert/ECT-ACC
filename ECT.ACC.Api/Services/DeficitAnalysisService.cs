using ECT.ACC.Api.Clients;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Math;
using ECT.ACC.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Services;

public class DeficitAnalysisService : IDeficitAnalysisService
{
    private readonly ECTDbContext _context;
    private readonly IGraphApiClient _graphClient;

    public DeficitAnalysisService(ECTDbContext context, IGraphApiClient graphClient)
    {
        _context = context;
        _graphClient = graphClient;
    }

    public async Task<DeficitAnalysisDto?> GetByScenarioIdAsync(int scenarioId)
    {
        var analysis = await _context.DeficitAnalyses
            .FirstOrDefaultAsync(d => d.ScenarioId == scenarioId);

        return analysis is null ? null : MapToDto(analysis);
    }

    public async Task<DeficitAnalysisDto> ComputeAndSaveAsync(int scenarioId)
    {
        var scenario = await _context.Scenarios
            .Include(s => s.Parameters)
            .FirstOrDefaultAsync(s => s.Id == scenarioId);

        if (scenario is null || scenario.Parameters is null)
            throw new InvalidOperationException(
                $"Scenario {scenarioId} not found or has no parameters.");

        var cRequired = ECTMath.SolveForC(
            scenario.Parameters.Complexity,
            scenario.Parameters.Energy,
            scenario.Parameters.TimeAvailable);

        var cAvailable = scenario.Parameters.Control;

        var cDeficit = ECTMath.ComputeDeficit(cRequired, cAvailable);
        var deficitType = ECTMath.ClassifyDeficit(cDeficit);

        var existing = await _context.DeficitAnalyses
            .FirstOrDefaultAsync(d => d.ScenarioId == scenarioId);

        if (existing is not null)
            _context.DeficitAnalyses.Remove(existing);

        var analysis = new DeficitAnalysis
        {
            ScenarioId = scenarioId,
            CRequired = cRequired,
            CAvailable = cAvailable,
            CDeficit = cDeficit,
            DeficitType = deficitType,
            ClassificationNotes = $"Computed via ECTMath on {DateTime.UtcNow:O}"
        };

        _context.DeficitAnalyses.Add(analysis);
        await _context.SaveChangesAsync();

        return MapToDto(analysis);
    }

    public async Task<DeficitAnalysisDto> ComputeAndSaveFromRollupAsync(
        int scenarioId,
        int configurationId,
        ScientificValue energy,
        ScientificValue control,
        ScientificValue complexity,
        ScientificValue timeAvailable)
    {
        var cRequired = ECTMath.SolveForC(complexity, energy, timeAvailable);
        var cAvailable = control;
        var cDeficit = ECTMath.ComputeDeficit(cRequired, cAvailable);
        var deficitType = ECTMath.ClassifyDeficit(cDeficit);

        var existing = await _context.DeficitAnalyses
            .FirstOrDefaultAsync(d => d.ScenarioId == scenarioId
                                   && d.ConfigurationId == configurationId);

        if (existing is not null)
            _context.DeficitAnalyses.Remove(existing);

        var analysis = new DeficitAnalysis
        {
            ScenarioId = scenarioId,
            ConfigurationId = configurationId,
            CRequired = cRequired,
            CAvailable = cAvailable,
            CDeficit = cDeficit,
            DeficitType = deficitType,
            ClassificationNotes = $"Computed from rollup on {DateTime.UtcNow:O}",
        };

        _context.DeficitAnalyses.Add(analysis);
        await _context.SaveChangesAsync();

        return MapToDto(analysis);
    }

    /// <summary>
    /// V2 graph-backed compute. Delegates rollup to ECT.Graph.Api,
    /// then applies ECTMath solve-for logic and persists the result.
    /// The solve-for mode is resolved by the graph service from the
    /// ScenarioNode — it comes back in the walk result.
    /// </summary>
    public async Task<DeficitAnalysisDto> ComputeAndSaveFromGraphAsync(
        int scenarioId,
        int configurationId,
        string scenarioGraphId,
        string configurationGraphId,
        string domain)
    {
        var walk = await _graphClient.GetConfigurationWalkAsync(
            scenarioGraphId,
            configurationGraphId);

        var (cRequired, cAvailable) = walk.SolveForMode switch
        {
            "C" => (
                ECTMath.SolveForC(walk.Complexity, walk.Energy, walk.TimeAvailable),
                walk.Control),

            "C_FromET" => (
                ECTMath.SolveForC_FromETProduct(
                    walk.Complexity,
                    ScientificValue.Multiply(walk.Energy, walk.TimeAvailable)),
                walk.Control),

            _ => throw new InvalidOperationException(
                $"SolveForMode '{walk.SolveForMode}' does not produce a control deficit. " +
                $"Use a dedicated endpoint for T, E, k, and combined solve-for modes.")
        };

        var cDeficit = ECTMath.ComputeDeficit(cRequired, cAvailable);
        var deficitType = ECTMath.ClassifyDeficit(cDeficit, walk.SolveForMode, domain);

        var existing = await _context.DeficitAnalyses
            .FirstOrDefaultAsync(d => d.ScenarioId == scenarioId
                                   && d.ConfigurationId == configurationId);

        if (existing is not null)
            _context.DeficitAnalyses.Remove(existing);

        var analysis = new DeficitAnalysis
        {
            ScenarioId = scenarioId,
            ConfigurationId = configurationId,
            CRequired = cRequired,
            CAvailable = cAvailable,
            CDeficit = cDeficit,
            DeficitType = deficitType,
            ClassificationNotes =
                $"V2 graph walk ({walk.SolveForMode}) on {DateTime.UtcNow:O}",
        };

        _context.DeficitAnalyses.Add(analysis);
        await _context.SaveChangesAsync();

        return MapToDto(analysis);
    }

    private static DeficitAnalysisDto MapToDto(DeficitAnalysis d) => new()
    {
        Id = d.Id,
        ScenarioId = d.ScenarioId,
        CRequired = new ScientificValueDto
        { Coefficient = d.CRequired.Coefficient, Exponent = d.CRequired.Exponent },
        CAvailable = new ScientificValueDto
        { Coefficient = d.CAvailable.Coefficient, Exponent = d.CAvailable.Exponent },
        CDeficit = new ScientificValueDto
        { Coefficient = d.CDeficit.Coefficient, Exponent = d.CDeficit.Exponent },
        DeficitType = d.DeficitType,
        ClassificationNotes = d.ClassificationNotes
    };
}