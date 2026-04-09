using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScenariosController : ControllerBase
{
    private readonly IScenarioService _scenarioService;
    private readonly IScenarioConfigurationService _configurationService;
    private readonly IHierarchicalScenarioService _hierarchicalService;
    private readonly IDeficitAnalysisService _deficitService;
    private readonly ECTDbContext _db;

    public ScenariosController(
        IScenarioService scenarioService,
        IScenarioConfigurationService configurationService,
        IHierarchicalScenarioService hierarchicalService,
        IDeficitAnalysisService deficitService,
        ECTDbContext db)
    {
        _scenarioService = scenarioService;
        _configurationService = configurationService;
        _hierarchicalService = hierarchicalService;
        _deficitService = deficitService;
        _db = db;
    }

    private async Task<Scenario?> GetScenarioEntityAsync(int id)
    {
        return await _db.Scenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScenarioDto>>> GetAll()
    {
        var scenarios = await _scenarioService.GetAllAsync();
        return Ok(scenarios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScenarioDto>> GetById(int id)
    {
        var scenario = await _scenarioService.GetByIdAsync(id);
        return scenario is null ? NotFound() : Ok(scenario);
    }

    [HttpPost]
    public async Task<ActionResult<ScenarioDto>> Create(CreateScenarioDto dto)
    {
        var created = await _scenarioService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScenarioDto>> Update(int id, UpdateScenarioDto dto)
    {
        var updated = await _scenarioService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPut("{id}/parameters")]
    public async Task<ActionResult<ScenarioParametersDto>> UpdateParameters(int id, UpdateScenarioParametersDto dto)
    {
        var updated = await _scenarioService.UpdateParametersAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _scenarioService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/rollup")]
    public async Task<ActionResult<object>> Rollup(int id)
    {
        try
        {
            // Get scenario to determine mode
            var scenario = await _scenarioService.GetByIdAsync(id);
            if (scenario is null)
                return NotFound($"Scenario {id} not found");

            // Ensure default configuration exists - need to get the full scenario entity
            var scenarioEntity = await GetScenarioEntityAsync(id);
            if (scenarioEntity is null)
                return NotFound($"Scenario entity {id} not found");

            await _configurationService.EnsureDefaultConfigurationAsync(scenarioEntity, _deficitService);

            // Get the default configuration
            var configs = await _configurationService.GetConfigurationsAsync(id);
            var defaultConfig = configs.FirstOrDefault(c => 
                c.Name == (scenario.ScenarioMode == "Hierarchical" ? "H - Rollup" : "Base"));

            if (defaultConfig is null)
                return BadRequest("Default configuration not found after ensuring it exists");

            // Branch by scenario mode
            if (scenario.ScenarioMode == "Hierarchical")
            {
                // 1. Run graph walk - source of truth for E, C, k, T
                var rollupResult = await _hierarchicalService.RollupAsync(id);

                // 2. Return rollup result with comprehensive null checks (deficit analysis should be separate process)
                return Ok(new 
                { 
                    mode = "Hierarchical",
                    rollupResult = new 
                    {
                        scenarioNodeId = rollupResult.ScenarioNodeId,
                        configurationNodeId = rollupResult.ConfigurationNodeId,
                        solveForMode = rollupResult.SolveForMode,
                        energy = rollupResult.Energy != null ? new { coefficient = rollupResult.Energy.Coefficient, exponent = rollupResult.Energy.Exponent } : null,
                        control = rollupResult.Control != null ? new { coefficient = rollupResult.Control.Coefficient, exponent = rollupResult.Control.Exponent } : null,
                        complexity = rollupResult.Complexity != null ? new { coefficient = rollupResult.Complexity.Coefficient, exponent = rollupResult.Complexity.Exponent } : null,
                        timeAvailable = rollupResult.TimeAvailable != null ? new { coefficient = rollupResult.TimeAvailable.Coefficient, exponent = rollupResult.TimeAvailable.Exponent } : null,
                        rootResult = rollupResult.RootResult != null ? new 
                        {
                            nodeId = rollupResult.RootResult.NodeId,
                            name = rollupResult.RootResult.Name,
                            role = rollupResult.RootResult.Role,
                            effectiveValue = rollupResult.RootResult.EffectiveValue != null ? new { coefficient = rollupResult.RootResult.EffectiveValue.Coefficient, exponent = rollupResult.RootResult.EffectiveValue.Exponent } : null,
                            weight = rollupResult.RootResult.Weight,
                            rollupOperator = rollupResult.RootResult.RollupOperator,
                            isLeaf = rollupResult.RootResult.IsLeaf,
                            children = rollupResult.RootResult.Children?.Select(child => new 
                            {
                                nodeId = child.NodeId,
                                name = child.Name,
                                role = child.Role,
                                effectiveValue = child.EffectiveValue != null ? new { coefficient = child.EffectiveValue.Coefficient, exponent = child.EffectiveValue.Exponent } : null,
                                weight = child.Weight,
                                rollupOperator = child.RollupOperator,
                                isLeaf = child.IsLeaf
                            }).ToList()
                        } : null,
                        rollupValue = rollupResult.RollupValue != null ? new { coefficient = rollupResult.RollupValue.Coefficient, exponent = rollupResult.RollupValue.Exponent } : null,
                        computedAt = rollupResult.ComputedAt
                    }
                });
            }
            else
            {
                // Flat: activate configuration and compute deficit
                await _configurationService.ActivateConfigurationAsync(
                    id, defaultConfig.Id, _deficitService);
                
                var deficit = await _configurationService.ComputeDeficitAnalysisAsync(
                    id, defaultConfig.Id, _deficitService);

                return Ok(new 
                { 
                    mode = "Flat",
                    configuration = defaultConfig,
                    deficitAnalysis = deficit
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Rollup failed", details = ex.Message });
        }
    }
}
