using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using ECT.ACC.Data.Math;
using Microsoft.EntityFrameworkCore;
using ECT.ACC.Api.Services;
using Microsoft.Extensions.Logging;

namespace ECT.ACC.Api.Services;

public class ScenarioService :IScenarioService
{
    private readonly ECTDbContext _context;
    private readonly IGraphManagementService _graphService;
    private readonly ILogger<ScenarioService> _logger;

    public ScenarioService(ECTDbContext context, IGraphManagementService graphService, ILogger<ScenarioService> logger)
    {  
        _context = context;
        _graphService = graphService;
        _logger = logger;
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
            ScenarioMode = dto.ScenarioMode, // "Flat" or "Hierarchical"
            SolveForMode = dto.SolveForMode,
            CreatedDate = DateTime.UtcNow
        };

        // --- STEP 1: ALWAYS MAP PARAMETERS TO SQL ---
        // Whether Flat or Hierarchical, the SQL DB needs these 8 values.
        if (dto.Parameters is not null)
        {
            scenario.Parameters = new ScenarioParameters
            {
                Energy = new ScientificValue(dto.Parameters.Energy.Coefficient, dto.Parameters.Energy.Exponent),
                Control = new ScientificValue(dto.Parameters.Control.Coefficient, dto.Parameters.Control.Exponent),
                Complexity = new ScientificValue(dto.Parameters.Complexity.Coefficient, dto.Parameters.Complexity.Exponent),
                TimeAvailable = new ScientificValue(dto.Parameters.TimeAvailable.Coefficient, dto.Parameters.TimeAvailable.Exponent)
            };
        }

        _context.Scenarios.Add(scenario);
        await _context.SaveChangesAsync();

        // --- STEP 2: CONDITIONAL GRAPH SYNC ---
        // Only 'Hierarchical' scenarios cross the bridge to Neo4j.
        if (scenario.ScenarioMode == "Hierarchical")
        {
            try
            {
                // 1. Create the Scenario Identity in Neo4j
                // This stops the "Scenario Not Found" errors.
                await _graphService.EnsureScenarioGraphExistsAsync(
                    scenario.Id,
                    scenario.Name,
                    scenario.Description ?? string.Empty,
                    scenario.SolveForMode,
                    scenario.ProcessDomainId?.ToString() ?? "Global"
                );

                // 2. Seed the 8 Scientific Parameters (E, T, C, k)
                // This ensures the "Add Step" logic has its base values immediately.
                var usesDto = new UsesEdgeDto
                {
                    ScenarioNodeId = scenario.Id.ToString(),
                    RootParameterNodeId = $"root-{scenario.Id}",
                    BaseParameterValues = MapToGraphValues(scenario.Parameters)
                };

                await _graphService.UpsertUsesEdgeAsync(scenario.Id, usesDto);
            }
            catch (Exception ex)
            {
                // Log it—the "Self-Healing" GET we added to the Graph API earlier
                // will act as the final safety net if this fails!
                _logger.LogWarning(ex, "Initial Graph sync for Scenario {Id} was incomplete.", scenario.Id);
            }
        }
        return MapToDto(scenario);
    }

    private static Dictionary<string, ScientificValueDto> MapToGraphValues(ScenarioParameters? p)
    {
        if (p == null) return new Dictionary<string, ScientificValueDto>();

        return new Dictionary<string, ScientificValueDto>
            {
                { "E", new ScientificValueDto { Coefficient = p.Energy.Coefficient, Exponent = p.Energy.Exponent } },
                { "T", new ScientificValueDto { Coefficient = p.TimeAvailable.Coefficient, Exponent = p.TimeAvailable.Exponent } },
                { "C", new ScientificValueDto { Coefficient = p.Complexity.Coefficient, Exponent = p.Complexity.Exponent } },
                { "k", new ScientificValueDto { Coefficient = p.Control.Coefficient, Exponent = p.Control.Exponent } }
            };
    }

    public async Task<ScenarioDto?> UpdateAsync(int id, UpdateScenarioDto dto)
    {
        var scenario = await _context.Scenarios.FindAsync(id);

        if (scenario is null) return null;

        if (dto.Name is not null) scenario.Name = dto.Name;
        if (dto.Description is not null) scenario.Description = dto.Description;
        if (dto.ScenarioMode is not null) scenario.ScenarioMode = dto.ScenarioMode;
        if (dto.SolveForMode is not null) scenario.SolveForMode = dto.SolveForMode;

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

        // Add these missing property mappings
        ScenarioMode = scenario.ScenarioMode,
        SolveForMode = scenario.SolveForMode,

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
