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

        // 1. Create the step anchor node first
        var stepAnchor = await _hierarchyService.CreateStepAsync(scenarioId, new CreateHierarchicalStepDto
        {
            Key = Guid.NewGuid().ToString(),
            Name = fullStepDto.StepName,
            Label = fullStepDto.StepName,
            Description = fullStepDto.Description ?? string.Empty,
            Role = "k",
            RollupOperator = "WeightedSum",
            Weight = 1.0,
            ParentNodeId = fullStepDto.ParentNodeId
        });

        // 2. Create the four leaf parameter nodes parented to the step anchor
        var createdLeaves = new List<HierarchicalStepDto>();
        foreach (var param in fullStepDto.Parameters)
        {
            param.Key = $"{stepAnchor.NodeId}-{param.Role}";
            param.Name = $"{fullStepDto.StepName} - {param.Role}";
            param.Label = param.Role;
            param.ParentNodeId = stepAnchor.NodeId;

            var leaf = await _hierarchyService.CreateStepAsync(scenarioId, param);
            createdLeaves.Add(leaf);
        }

        return Ok(new { step = stepAnchor, parameters = createdLeaves });
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
        try
        {
            var result = await _hierarchyService.RollupAsync(scenarioId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Rollup failed", details = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
