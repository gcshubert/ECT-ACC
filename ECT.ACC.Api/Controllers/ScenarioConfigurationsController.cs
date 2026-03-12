using Microsoft.AspNetCore.Mvc;
using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;

namespace ECT.ACC.Api.Controllers;

/// <summary>
/// Phase 5 — Scenario Configurations
///
/// All routes are nested under /api/Scenarios/{scenarioId}/configurations
///
/// GET    /api/Scenarios/{scenarioId}/configurations
/// POST   /api/Scenarios/{scenarioId}/configurations
/// GET    /api/Scenarios/{scenarioId}/configurations/{configId}
/// PUT    /api/Scenarios/{scenarioId}/configurations/{configId}
/// DELETE /api/Scenarios/{scenarioId}/configurations/{configId}
/// PUT    /api/Scenarios/{scenarioId}/configurations/{configId}/entries/{paramKey}
/// POST   /api/Scenarios/{scenarioId}/configurations/{configId}/activate
/// </summary>
[ApiController]
[Route("api/Scenarios/{scenarioId:int}/configurations")]
public class ScenarioConfigurationsController : ControllerBase
{
    private readonly IScenarioConfigurationService _service;
    private readonly IDeficitAnalysisService       _deficitService;

    public ScenarioConfigurationsController(
        IScenarioConfigurationService service,
        IDeficitAnalysisService deficitService)
    {
        _service        = service;
        _deficitService = deficitService;
    }

    // ── GET all ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<List<ScenarioConfigurationDto>>> GetAll(int scenarioId)
    {
        var result = await _service.GetConfigurationsAsync(scenarioId);
        return Ok(result);
    }

    // ── GET one ──────────────────────────────────────────────────────────────

    [HttpGet("{configId:int}")]
    public async Task<ActionResult<ScenarioConfigurationDto>> GetOne(int scenarioId, int configId)
    {
        var result = await _service.GetConfigurationAsync(scenarioId, configId);
        return result is null ? NotFound() : Ok(result);
    }

    // ── POST (create / clone) ─────────────────────────────────────────────────

    [HttpPost]
    public async Task<ActionResult<ScenarioConfigurationDto>> Create(
        int scenarioId,
        [FromBody] CreateScenarioConfigurationRequest request)
    {
        var result = await _service.CreateConfigurationAsync(scenarioId, request);
        return CreatedAtAction(nameof(GetOne),
            new { scenarioId, configId = result.Id },
            result);
    }

    // ── PUT (rename / redescribe / reorder) ───────────────────────────────────

    [HttpPut("{configId:int}")]
    public async Task<ActionResult<ScenarioConfigurationDto>> Update(
        int scenarioId, int configId,
        [FromBody] UpdateScenarioConfigurationRequest request)
    {
        var result = await _service.UpdateConfigurationAsync(scenarioId, configId, request);
        return result is null ? NotFound() : Ok(result);
    }

    // ── PUT entry (swap variant for one parameter) ────────────────────────────

    [HttpPut("{configId:int}/entries/{paramKey}")]
    public async Task<ActionResult<ScenarioConfigurationDto>> UpdateEntry(
        int scenarioId, int configId, string paramKey,
        [FromBody] UpdateConfigurationEntryRequest request)
    {
        var result = await _service.UpdateEntryAsync(scenarioId, configId, paramKey, request);
        return result is null ? NotFound() : Ok(result);
    }

    // ── POST activate (apply all entries + recompute deficit) ─────────────────

    [HttpPost("{configId:int}/activate")]
    public async Task<ActionResult<ScenarioConfigurationDto>> Activate(
        int scenarioId, int configId)
    {
        var result = await _service.ActivateConfigurationAsync(
            scenarioId, configId, _deficitService);
        return result is null ? NotFound() : Ok(result);
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    [HttpDelete("{configId:int}")]
    public async Task<IActionResult> Delete(int scenarioId, int configId)
    {
        var deleted = await _service.DeleteConfigurationAsync(scenarioId, configId);
        return deleted ? NoContent() : NotFound();
    }
}
