using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Math;
using ECT.ACC.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/Scenarios/{scenarioId:int}/parameter-definitions")]
public class ParameterDefinitionsController : ControllerBase
{
    private readonly ECTDbContext _db;
    public ParameterDefinitionsController(ECTDbContext db) => _db = db;

    // ── GET all definitions for a scenario ────────────────────────────────────
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ParameterDefinitionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int scenarioId)
    {
        if (!await _db.Scenarios.AnyAsync(s => s.Id == scenarioId))
            return NotFound();

        var defs = await _db.ParameterDefinitions
            .Where(d => d.ScenarioId == scenarioId)
            .OrderBy(d => d.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        return Ok(defs.Select(MapDef).ToList());
    }

    // ── POST: add a new parameter definition ─────────────────────────────────
    [HttpPost]
    [ProducesResponseType<ParameterDefinitionDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        int scenarioId,
        [FromBody] CreateParameterDefinitionRequest req)
    {
        if (!await _db.Scenarios.AnyAsync(s => s.Id == scenarioId))
            return NotFound();

        var def = new ParameterDefinition
        {
            ScenarioId         = scenarioId,
            Key                = req.Key,
            Symbol             = req.Symbol,
            Label              = req.Label,
            Description        = req.Description,
            Unit               = req.Unit,
            SortOrder          = req.SortOrder,
            IsEctCoreParameter = req.IsEctCoreParameter,
            DefaultValue       = req.DefaultValue is null ? null
                : new ScientificValueOwned
                    { Coefficient = req.DefaultValue.Coefficient,
                      Exponent    = (int)req.DefaultValue.Exponent },
        };

        _db.ParameterDefinitions.Add(def);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { scenarioId }, MapDef(def));
    }

    // ── PUT: update an existing definition ───────────────────────────────────
    [HttpPut("{defId:int}")]
    [ProducesResponseType<ParameterDefinitionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int scenarioId, int defId,
        [FromBody] UpdateParameterDefinitionRequest req)
    {
        var def = await _db.ParameterDefinitions
            .FirstOrDefaultAsync(d => d.Id == defId && d.ScenarioId == scenarioId);
        if (def is null) return NotFound();

        def.Symbol       = req.Symbol;
        def.Label        = req.Label;
        def.Description  = req.Description;
        def.Unit         = req.Unit;
        def.SortOrder    = req.SortOrder;
        def.DefaultValue = req.DefaultValue is null ? null
            : new ScientificValueOwned
                { Coefficient = req.DefaultValue.Coefficient,
                  Exponent    = (int)req.DefaultValue.Exponent };

        await _db.SaveChangesAsync();
        return Ok(MapDef(def));
    }

    // ── DELETE: remove a parameter definition ────────────────────────────────
    [HttpDelete("{defId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int scenarioId, int defId)
    {
        var def = await _db.ParameterDefinitions
            .FirstOrDefaultAsync(d => d.Id == defId && d.ScenarioId == scenarioId);
        if (def is null) return NotFound();

        _db.ParameterDefinitions.Remove(def);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST: apply a template ────────────────────────────────────────────────
    [HttpPost("apply-template")]
    [ProducesResponseType<IReadOnlyList<ParameterDefinitionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyTemplate(
        int scenarioId,
        [FromBody] ApplyTemplateRequest req)
    {
        if (!await _db.Scenarios.AnyAsync(s => s.Id == scenarioId))
            return NotFound();

        var template = await _db.ParameterTemplates
            .Include(t => t.ParameterDefinitions)
            .FirstOrDefaultAsync(t => t.Id == req.TemplateId);
        if (template is null) return NotFound();

        // Remove existing definitions before applying template
        var existing = _db.ParameterDefinitions.Where(d => d.ScenarioId == scenarioId);
        _db.ParameterDefinitions.RemoveRange(existing);

        // Clone template definitions into the scenario
        var cloned = template.ParameterDefinitions
            .OrderBy(p => p.SortOrder)
            .Select(p => new ParameterDefinition
            {
                ScenarioId         = scenarioId,
                Key                = p.Key,
                Symbol             = p.Symbol,
                Label              = p.Label,
                Description        = p.Description,
                Unit               = p.DefaultUnit,
                SortOrder          = p.SortOrder,
                IsEctCoreParameter = p.IsEctCoreParameter,
                DefaultValue       = p.SeedValue is null ? null
                    : new ScientificValueOwned
                        { Coefficient = p.SeedValue.Coefficient,
                          Exponent    = p.SeedValue.Exponent },
            })
            .ToList();

        _db.ParameterDefinitions.AddRange(cloned);

        // Stamp the domain on the Scenario
        var scenario = await _db.Scenarios.FindAsync(scenarioId);
        if (scenario is not null)
            scenario.ProcessDomainId = template.ProcessDomainId;

        await _db.SaveChangesAsync();
        return Ok(cloned.Select(MapDef).ToList());
    }

    // ── GET rollup: evaluate derivation chain for a parameter ─────────────────
    [HttpGet("{paramKey}/rollup")]
    [ProducesResponseType<RollupResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRollup(int scenarioId, string paramKey)
    {
        var doc = await _db.ParameterDocumentations
            .Include(d => d.SubParameters.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(d =>
                d.ScenarioId   == scenarioId &&
                d.ParameterKey == paramKey);

        if (doc is null) return NotFound();

        var steps = doc.SubParameters
            .OrderBy(s => s.StepOrder)
            .ToList();

        var rollupSteps = new List<RollupStepDto>();
        var runningSteps = new List<(double Coefficient, int Exponent, StepOperation Op)>();

        foreach (var s in steps)
        {
            runningSteps.Add((s.Value.Coefficient, (int)s.Value.Exponent, s.Operation));
            var running = DerivationRollupService.Compute(runningSteps);
            rollupSteps.Add(new RollupStepDto(
                s.StepOrder, s.Name,
                new ScientificValueDto { Coefficient = s.Value.Coefficient, Exponent = s.Value.Exponent },
                (StepOperationDto)s.Operation,
                new ScientificValueDto { Coefficient = s.Value.Coefficient, Exponent = s.Value.Exponent }
            ));
        }

        var final = DerivationRollupService.Compute(
            steps.Select(s => (s.Value.Coefficient, (int)s.Value.Exponent, s.Operation)));

        return Ok(new RollupResultDto(
                    paramKey,
                    new ScientificValueDto { Coefficient = final.Coefficient, Exponent = final.Exponent },
                    rollupSteps
                ));
    }

    // ── Mapping ───────────────────────────────────────────────────────────────
    private static ParameterDefinitionDto MapDef(ParameterDefinition d) => new(
        d.Id, d.Key, d.Symbol, d.Label, d.Description, d.Unit,
        d.SortOrder, d.IsEctCoreParameter,
        d.DefaultValue is null ? null
            : new ScientificValueDto { Coefficient = d.DefaultValue.Coefficient, Exponent = d.DefaultValue.Exponent }
    );
}
