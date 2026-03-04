using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Math;
using ECT.ACC.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Services;

public class DeficitAnalysisService : IDeficitAnalysisService
{
    private readonly ECTDbContext _context;

    public DeficitAnalysisService(ECTDbContext context)
    {
        _context = context;
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

        // Use ECTMath to compute values
        var cRequired = ECTMath.ComputeMinimumControl(
            scenario.Parameters.Complexity,
            scenario.Parameters.Energy,
            scenario.Parameters.TimeAvailable);

        var cAvailable = scenario.Parameters.Control;

        var cDeficit = ECTMath.ComputeDeficit(cRequired, cAvailable);
        var deficitType = ECTMath.ClassifyDeficit(cDeficit);

        // Remove existing analysis if present
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

    private static DeficitAnalysisDto MapToDto(DeficitAnalysis d) => new()
    {
        Id = d.Id,
        ScenarioId = d.ScenarioId,
        CRequired = new ScientificValueDto { Coefficient = d.CRequired.Coefficient, Exponent = d.CRequired.Exponent },
        CAvailable = new ScientificValueDto { Coefficient = d.CAvailable.Coefficient, Exponent = d.CAvailable.Exponent },
        CDeficit = new ScientificValueDto { Coefficient = d.CDeficit.Coefficient, Exponent = d.CDeficit.Exponent },
        DeficitType = d.DeficitType,
        ClassificationNotes = d.ClassificationNotes
    };
}
