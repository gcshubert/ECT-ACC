using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using ECT.ACC.Data.Math;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Controllers;

/// <summary>
/// Manages derivation documentation for the four ECT parameters (E, C, k, T)
/// on a per-scenario basis.
///
/// Base route: /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation
///
/// Endpoints:
///   GET    /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation
///   PUT    /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation
///   POST   /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/sub-parameters
///   PUT    /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/sub-parameters/{stepId}
///   DELETE /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/sub-parameters/{stepId}
///   POST   /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/variants
///   DELETE /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/variants/{variantId}
///   POST   /api/Scenarios/{scenarioId}/parameters/{paramKey}/documentation/variants/activate
/// </summary>
[ApiController]
[Route("api/Scenarios/{scenarioId:int}/parameters/{paramKey}/documentation")]
public class ParameterDocumentationController : ControllerBase
{
    private readonly ECTDbContext _db;

    public ParameterDocumentationController(ECTDbContext db) => _db = db;

    // ─── GET: fetch full doc for one parameter ───────────────────────────────
    [HttpGet]
    [ProducesResponseType<ParameterDocumentationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int scenarioId, string paramKey)
    {
        var doc = await FindDocAsync(scenarioId, paramKey);
        return doc is null ? NotFound() : Ok(MapDoc(doc));
    }

    // ─── PUT: upsert the top-level doc + replace all base sub-parameters ─────
    [HttpPut]
    [ProducesResponseType<ParameterDocumentationDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert(
        int scenarioId, string paramKey,
        [FromBody] UpsertParameterDocumentationRequest request)
    {
        if (!await _db.Scenarios.AnyAsync(s => s.Id == scenarioId))
            return NotFound();

        var doc = await FindDocAsync(scenarioId, paramKey);

        if (doc is null)
        {
            doc = new ParameterDocumentation
            {
                ScenarioId    = scenarioId,
                ParameterKey  = paramKey,
            };
            _db.ParameterDocumentations.Add(doc);
        }

        doc.Label                = request.Label;
        doc.DerivationNarrative  = request.DerivationNarrative;

        // Replace base sub-parameters
        _db.SubParameters.RemoveRange(doc.SubParameters);
        doc.SubParameters = request.SubParameters
            .OrderBy(s => s.StepOrder)
            .Select(s => MapCreateRequest(s))
            .ToList();

        await _db.SaveChangesAsync();

        // Reload with nav props
        doc = await FindDocAsync(scenarioId, paramKey);
        return Ok(MapDoc(doc!));
    }

    // ─── POST: append a single sub-parameter step ────────────────────────────
    [HttpPost("sub-parameters")]
    [ProducesResponseType<SubParameterDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddStep(
        int scenarioId, string paramKey,
        [FromBody] CreateSubParameterRequest request)
    {
        var doc = await FindDocAsync(scenarioId, paramKey)
                  ?? await CreateEmptyDocAsync(scenarioId, paramKey);

        var step = MapCreateRequest(request);
        step.ParameterDocumentationId = doc.Id;
        _db.SubParameters.Add(step);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get),
            new { scenarioId, paramKey }, MapStep(step));
    }

    // ─── PUT: update a single sub-parameter step ─────────────────────────────
    [HttpPut("sub-parameters/{stepId:int}")]
    [ProducesResponseType<SubParameterDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStep(
        int scenarioId, string paramKey, int stepId,
        [FromBody] UpdateSubParameterRequest request)
    {
        var step = await _db.SubParameters
            .Include(s => s.ParameterDocumentation)
            .FirstOrDefaultAsync(s => s.Id == stepId
                && s.ParameterDocumentation.ScenarioId == scenarioId
                && s.ParameterDocumentation.ParameterKey == paramKey);

        if (step is null) return NotFound();

        step.StepOrder       = request.StepOrder;
        step.Name            = request.Name;
        step.Value           = new ScientificValueOwned
            { Coefficient = request.Value.Coefficient, Exponent = request.Value.Exponent };
        step.Unit            = request.Unit;
        step.Rationale       = request.Rationale;
        step.SourceReference = request.SourceReference;

        await _db.SaveChangesAsync();
        return Ok(MapStep(step));
    }

    // ─── DELETE: remove a sub-parameter step ─────────────────────────────────
    [HttpDelete("sub-parameters/{stepId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStep(
        int scenarioId, string paramKey, int stepId)
    {
        var step = await _db.SubParameters
            .Include(s => s.ParameterDocumentation)
            .FirstOrDefaultAsync(s => s.Id == stepId
                && s.ParameterDocumentation.ScenarioId == scenarioId
                && s.ParameterDocumentation.ParameterKey == paramKey);

        if (step is null) return NotFound();
        _db.SubParameters.Remove(step);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST: create a named variant ────────────────────────────────────────
    [HttpPost("variants")]
    [ProducesResponseType<ParameterVariantDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddVariant(
        int scenarioId, string paramKey,
        [FromBody] CreateParameterVariantRequest request)
    {
        var doc = await FindDocAsync(scenarioId, paramKey)
                  ?? await CreateEmptyDocAsync(scenarioId, paramKey);

        var variant = new ParameterVariant
        {
            ParameterDocumentationId = doc.Id,
            Name                     = request.Name,
            IsActive                 = false,
            SubParameters            = request.SubParameters
                .OrderBy(s => s.StepOrder)
                .Select(s => new VariantSubParameter
                {
                    StepOrder       = s.StepOrder,
                    Name            = s.Name,
                    Value           = new ScientificValueOwned
                        { Coefficient = s.Value.Coefficient, Exponent = s.Value.Exponent },
                    Unit            = s.Unit,
                    Rationale       = s.Rationale,
                    SourceReference = s.SourceReference,
                })
                .ToList(),
        };

        _db.ParameterVariants.Add(variant);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get),
            new { scenarioId, paramKey }, MapVariant(variant));
    }

    // ─── PUT: update a variant sub-parameter step ────────────────────────────
    [HttpPut("variants/{variantId:int}/sub-parameters/{stepId:int}")]
    [ProducesResponseType<VariantSubParameterDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariantStep(
        int scenarioId, string paramKey, int variantId, int stepId,
        [FromBody] UpdateSubParameterRequest request)
    {
        var step = await _db.VariantSubParameters
            .Include(s => s.ParameterVariant)
                .ThenInclude(v => v.ParameterDocumentation)
            .FirstOrDefaultAsync(s => s.Id == stepId
                && s.ParameterVariant.Id == variantId
                && s.ParameterVariant.ParameterDocumentation.ScenarioId == scenarioId
                && s.ParameterVariant.ParameterDocumentation.ParameterKey == paramKey);

        if (step is null) return NotFound();

        step.StepOrder       = request.StepOrder;
        step.Name            = request.Name;
        step.Value           = new ScientificValueOwned
            { Coefficient = request.Value.Coefficient, Exponent = request.Value.Exponent };
        step.Unit            = request.Unit;
        step.Rationale       = request.Rationale;
        step.SourceReference = request.SourceReference;
        step.Operation       = (StepOperation)request.Operation;

        await _db.SaveChangesAsync();
        return Ok(new VariantSubParameterDto(
            step.Id, step.StepOrder, step.Name,
            new ScientificValueDto { Coefficient = step.Value.Coefficient, Exponent = step.Value.Exponent },
            step.Unit, step.Rationale, step.SourceReference,
            (StepOperationDto)step.Operation
        ));
    }

    // ─── DELETE: remove a variant ────────────────────────────────────────────
    [HttpDelete("variants/{variantId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariant(
        int scenarioId, string paramKey, int variantId)
    {
        var variant = await _db.ParameterVariants
            .Include(v => v.ParameterDocumentation)
            .FirstOrDefaultAsync(v => v.Id == variantId
                && v.ParameterDocumentation.ScenarioId == scenarioId
                && v.ParameterDocumentation.ParameterKey == paramKey);

        if (variant is null) return NotFound();
        _db.ParameterVariants.Remove(variant);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST: activate a variant (sets IsActive, clears others) ─────────────
    [HttpPost("variants/activate")]
    [ProducesResponseType<ParameterDocumentationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateVariant(
        int scenarioId, string paramKey,
        [FromBody] ActivateVariantRequest request)
    {
        var doc = await FindDocAsync(scenarioId, paramKey);
        if (doc is null) return NotFound();

        foreach (var v in doc.Variants)
            v.IsActive = v.Id == request.VariantId;

        await _db.SaveChangesAsync();
        doc = await FindDocAsync(scenarioId, paramKey);
        return Ok(MapDoc(doc!));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<ParameterDocumentation?> FindDocAsync(
        int scenarioId, string paramKey) =>
        await _db.ParameterDocumentations
            .Include(d => d.SubParameters.OrderBy(s => s.StepOrder))
            .Include(d => d.Variants)
                .ThenInclude(v => v.SubParameters.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(d =>
                d.ScenarioId   == scenarioId &&
                d.ParameterKey == paramKey);

    private async Task<ParameterDocumentation> CreateEmptyDocAsync(
        int scenarioId, string paramKey)
    {
        var doc = new ParameterDocumentation
        {
            ScenarioId           = scenarioId,
            ParameterKey         = paramKey,
            Label                = paramKey.ToUpperInvariant(),
            DerivationNarrative  = string.Empty,
        };
        _db.ParameterDocumentations.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    private static SubParameter MapCreateRequest(CreateSubParameterRequest r) => new()
    {
        StepOrder       = r.StepOrder,
        Name            = r.Name,
        Value           = new ScientificValueOwned
            { Coefficient = r.Value.Coefficient, Exponent = r.Value.Exponent },
        Unit            = r.Unit,
        Rationale       = r.Rationale,
        SourceReference = r.SourceReference,
    };

    // ── Mapping to DTOs ───────────────────────────────────────────────────────

    private static ParameterDocumentationDto MapDoc(ParameterDocumentation d) => new(
        d.Id,
        d.ParameterKey,
        d.Label,
        d.DerivationNarrative,
        d.SubParameters.OrderBy(s => s.StepOrder).Select(MapStep).ToList(),
        d.Variants.Select(MapVariant).ToList()
    );

    private static SubParameterDto MapStep(SubParameter s) => new(
            s.Id,
            s.StepOrder,
            s.Name,
            new ScientificValueDto { Coefficient = s.Value.Coefficient, Exponent = s.Value.Exponent },
            s.Unit,
            s.Rationale,
            s.SourceReference,
            (StepOperationDto)s.Operation
        );

    private static ParameterVariantDto MapVariant(ParameterVariant v) => new(
        v.Id,
        v.Name,
        v.IsActive,
        v.SubParameters.OrderBy(s => s.StepOrder).Select(s => new VariantSubParameterDto(
            s.Id, s.StepOrder, s.Name,
            new ScientificValueDto { Coefficient = s.Value.Coefficient, Exponent = s.Value.Exponent },
            s.Unit, s.Rationale, s.SourceReference,
            (StepOperationDto)s.Operation
        )).ToList()
    );
}