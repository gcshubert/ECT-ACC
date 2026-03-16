using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeficitAnalysisController : ControllerBase
{
    private readonly IDeficitAnalysisService _deficitAnalysisService;

    public DeficitAnalysisController(IDeficitAnalysisService deficitAnalysisService)
    {
        _deficitAnalysisService = deficitAnalysisService;
    }

    // -------------------------------------------------------------------------
    // V1 endpoints — retained unchanged
    // -------------------------------------------------------------------------

    [HttpGet("scenario/{scenarioId}")]
    public async Task<ActionResult<DeficitAnalysisDto>> GetByScenarioId(int scenarioId)
    {
        var analysis = await _deficitAnalysisService.GetByScenarioIdAsync(scenarioId);
        return analysis is null ? NotFound() : Ok(analysis);
    }

    [HttpPost("scenario/{scenarioId}/compute")]
    public async Task<ActionResult<DeficitAnalysisDto>> Compute(int scenarioId)
    {
        try
        {
            var analysis = await _deficitAnalysisService.ComputeAndSaveAsync(scenarioId);
            return Ok(analysis);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // -------------------------------------------------------------------------
    // V2 endpoints — graph-backed, configuration-level, solve-for-mode aware
    // -------------------------------------------------------------------------

    /// <summary>
    /// V2 deficit compute — delegates rollup to ECT.Graph.Api.
    ///
    /// scenarioGraphId and configurationGraphId are the UUID node identifiers
    /// in Neo4j (not the SQL Server integer IDs). The solve-for mode is
    /// resolved by the graph service from the ScenarioNode.
    ///
    /// Only valid for C and C_FromET solve-for modes. Returns 400 for
    /// T, E, k, and combined modes — those do not produce a control deficit.
    /// </summary>
    [HttpPost("scenario/{scenarioId}/configuration/{configurationId}/compute-v2")]
    public async Task<ActionResult<DeficitAnalysisDto>> ComputeV2(
        int scenarioId,
        int configurationId,
        [FromQuery] string scenarioGraphId,
        [FromQuery] string configurationGraphId,
        [FromQuery] string domain)
    {
        if (string.IsNullOrWhiteSpace(scenarioGraphId))
            return BadRequest("scenarioGraphId is required.");
        if (string.IsNullOrWhiteSpace(configurationGraphId))
            return BadRequest("configurationGraphId is required.");
        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest("domain is required.");

        try
        {
            var analysis = await _deficitAnalysisService.ComputeAndSaveFromGraphAsync(
                scenarioId,
                configurationId,
                scenarioGraphId,
                configurationGraphId,
                domain);

            return Ok(analysis);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}