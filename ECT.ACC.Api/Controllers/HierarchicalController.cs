using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Math;
using Microsoft.AspNetCore.Mvc;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/scenarios/{scenarioId}/hierarchy")]
public class HierarchicalController : ControllerBase
{
    private readonly IHierarchicalScenarioService _hierarchyService;

    public HierarchicalController(IHierarchicalScenarioService hierarchyService)
    {
        _hierarchyService = hierarchyService;
    }

    [HttpGet("steps")]
    public async Task<ActionResult<IEnumerable<HierarchicalStepDto>>> GetSteps(int scenarioId)
    {
        var steps = await _hierarchyService.GetStepsAsync(scenarioId);
        return Ok(steps);
    }
    [HttpPost("hierarchical-steps")]
    public async Task<ActionResult> CreateFullStep(
    int scenarioId,
    [FromBody] CreateHierarchicalStepWithParametersDto fullStepDto)
    {
        if (fullStepDto.Parameters.Count != 4)
            return BadRequest("A Hierarchical Step must include exactly 4 parameters (E, T, C, k).");

        return Ok();
    }



    [HttpPost("steps")]
    public async Task<ActionResult<HierarchicalStepDto>> CreateStep(int scenarioId, CreateHierarchicalStepDto dto)
    {
        var step = await _hierarchyService.CreateStepAsync(scenarioId, dto);
        return CreatedAtAction(nameof(GetSteps), new { scenarioId }, step);
    }

    [HttpPut("steps/{stepId}")]
    public async Task<ActionResult<HierarchicalStepDto>> UpdateStep(int scenarioId, string stepId, UpdateHierarchicalStepDto dto)
    {
        var step = await _hierarchyService.UpdateStepAsync(scenarioId, stepId, dto);
        return step is null ? NotFound() : Ok(step);
    }

    [HttpDelete("steps/{stepId}")]
    public async Task<IActionResult> DeleteStep(int scenarioId, string stepId)
    {
        var deleted = await _hierarchyService.DeleteStepAsync(scenarioId, stepId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("rollup")]
    public async Task<ActionResult<GraphWalkResultTree>> Rollup(int scenarioId)
    {
        var result = await _hierarchyService.RollupAsync(scenarioId);
        return Ok(result);
    }
}
