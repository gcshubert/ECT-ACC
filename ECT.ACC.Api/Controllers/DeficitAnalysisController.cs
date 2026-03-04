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
}