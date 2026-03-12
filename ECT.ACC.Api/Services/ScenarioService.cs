using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using ECT.ACC.Data.Math;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Services;

public class ScenarioService :IScenarioService
{
    private readonly ECTDbContext _context;
    public ScenarioService(ECTDbContext context)
    {  
        _context = context; 
    }
    public async Task<IEnumerable<ScenarioDto>> GetAllAsync()
    {
        var scenarios = await _context.Scenarios
            .Include(s => s.Parameters)
            .Include(s => s.DeficitAnalyses)
            .ToListAsync();
        return scenarios.Select(MapToDto);
    }
    public async Task<ScenarioDto?> GetByIdAsync(int id)
    {
        var scenario = await _context.Scenarios
            .Include(s => s.Parameters)
            .Include(s => s.DeficitAnalyses)
            .FirstOrDefaultAsync(s => s.Id == id);

        return scenario is null ? null : MapToDto(scenario);
    }

    public async Task<ScenarioDto> CreateAsync(CreateScenarioDto dto)
    {
        var scenario = new Scenario
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedDate = DateTime.UtcNow
        };

        if (dto.Parameters is not null)
        {
            scenario.Parameters = new ScenarioParameters
            {
                Energy = new ScientificValue(
                    dto.Parameters.Energy.Coefficient,
                    dto.Parameters.Energy.Exponent),
                Control = new ScientificValue(
                    dto.Parameters.Control.Coefficient,
                    dto.Parameters.Control.Exponent),
                Complexity = new ScientificValue(
                    dto.Parameters.Complexity.Coefficient,
                    dto.Parameters.Complexity.Exponent),
                TimeAvailable = new ScientificValue(
                    dto.Parameters.TimeAvailable.Coefficient,
                    dto.Parameters.TimeAvailable.Exponent)
            };
        }
        _context.Scenarios.Add(scenario);
        await _context.SaveChangesAsync();

        return MapToDto(scenario);
    }

    public async Task<ScenarioDto?> UpdateAsync(int id, UpdateScenarioDto dto)
    {
        var scenario = await _context.Scenarios.FindAsync(id);

        if (scenario is null) return null;

        scenario.Name = dto.Name;
        scenario.Description = dto.Description;

        await _context.SaveChangesAsync();

        return MapToDto(scenario);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var scenario = await _context.Scenarios.FindAsync(id);

        if (scenario is null) return false;

        _context.Scenarios.Remove(scenario);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ScenarioParametersDto?> UpdateParametersAsync(int scenarioId, UpdateScenarioParametersDto dto)
    {
        var scenario = await _context.Scenarios
            .Include(s => s.Parameters)
            .FirstOrDefaultAsync(s => s.Id == scenarioId);

        if (scenario is null) return null;

        if (scenario.Parameters is null)
        {
            scenario.Parameters = new ScenarioParameters
            {
                ScenarioId = scenarioId,
                Energy = new ScientificValue(dto.Energy.Coefficient, dto.Energy.Exponent),
                Control = new ScientificValue(dto.Control.Coefficient, dto.Control.Exponent),
                Complexity = new ScientificValue(dto.Complexity.Coefficient, dto.Complexity.Exponent),
                TimeAvailable = new ScientificValue(dto.TimeAvailable.Coefficient, dto.TimeAvailable.Exponent)
            };
        }
        else
        {
            scenario.Parameters.Energy = new ScientificValue(dto.Energy.Coefficient, dto.Energy.Exponent);
            scenario.Parameters.Control = new ScientificValue(dto.Control.Coefficient, dto.Control.Exponent);
            scenario.Parameters.Complexity = new ScientificValue(dto.Complexity.Coefficient, dto.Complexity.Exponent);
            scenario.Parameters.TimeAvailable = new ScientificValue(dto.TimeAvailable.Coefficient, dto.TimeAvailable.Exponent);
        }

        await _context.SaveChangesAsync();

        return MapToDto(scenario.Parameters);
    }

    // Mapping helpers
    private static ScenarioDto MapToDto(Scenario scenario) => new()
    {
        Id = scenario.Id,
        Name = scenario.Name,
        Description = scenario.Description,
        CreatedDate = scenario.CreatedDate,
        ProcessDomainId = scenario.ProcessDomainId,
        Parameters = scenario.Parameters is null ? null : MapToDto(scenario.Parameters),
        DeficitAnalysis = scenario.DeficitAnalyses
            .OrderByDescending(d => d.Id)
            .Select(d => MapToDto(d))
            .FirstOrDefault()
    };

    private static ScenarioParametersDto MapToDto(ScenarioParameters p) => new()
    {
        Id = p.Id,
        ScenarioId = p.ScenarioId,
        Energy = new ScientificValueDto { Coefficient = p.Energy.Coefficient, Exponent = p.Energy.Exponent },
        Control = new ScientificValueDto { Coefficient = p.Control.Coefficient, Exponent = p.Control.Exponent },
        Complexity = new ScientificValueDto { Coefficient = p.Complexity.Coefficient, Exponent = p.Complexity.Exponent },
        TimeAvailable = new ScientificValueDto { Coefficient = p.TimeAvailable.Coefficient, Exponent = p.TimeAvailable.Exponent }
    };

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
