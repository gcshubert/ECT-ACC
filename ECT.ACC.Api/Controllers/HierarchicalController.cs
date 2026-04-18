using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Math;
using Microsoft.AspNetCore.Mvc;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/scenarios/{sid}/hierarchy")]
public class HierarchicalController : ControllerBase
{
    private readonly IHierarchicalScenarioService _hierarchyService;
    private readonly IGraphManagementService _graph;

    public HierarchicalController(IHierarchicalScenarioService hierarchyService, IGraphManagementService graph)
    {
        _hierarchyService = hierarchyService;
        _graph = graph;
    }

    [HttpGet("steps")]
    public async Task<ActionResult<IEnumerable<HierarchicalStepDto>>> GetSteps([FromRoute] int sid)
    {
        var steps = await _hierarchyService.GetStepsAsync(sid);
        return Ok(steps);
    }

    [HttpPost("hierarchical-steps")]
    public async Task<ActionResult> CreateFullStep(
        [FromRoute] int sid,
        [FromBody] CreateHierarchicalStepWithParametersDto fullStepDto)
    {
        if (fullStepDto.Parameters.Count != 4)
            return BadRequest("A Hierarchical Step must include exactly 4 parameters (E, T, C, k).");

        // Extract parameter values from the request
        var eParam = fullStepDto.Parameters.FirstOrDefault(p => p.Role == "E");
        var tParam = fullStepDto.Parameters.FirstOrDefault(p => p.Role == "T");
        var cParam = fullStepDto.Parameters.FirstOrDefault(p => p.Role == "C");
        var kParam = fullStepDto.Parameters.FirstOrDefault(p => p.Role == "k");

        // Create step anchor with parameters stored as properties
        var stepAnchor = await _hierarchyService.CreateStepAsync(sid, new CreateHierarchicalStepDto
        {
            Key = Guid.NewGuid().ToString(),
            Name = fullStepDto.StepName,
            Label = fullStepDto.StepName,
            Description = fullStepDto.Description ?? string.Empty,
            Role = "k",
            RollupOperator = "WeightedSum",
            Weight = 1.0,
            ParentNodeId = fullStepDto.ParentNodeId,
            // Store parameters directly on the step anchor
            E = eParam?.BaseValue,
            C = cParam?.BaseValue,
            K = kParam?.BaseValue,
            T = tParam?.BaseValue
        });

        return Ok(new { step = stepAnchor });
    }

    [HttpPost("steps")]
    public async Task<ActionResult<HierarchicalStepDto>> CreateStep([FromRoute] int sid, CreateHierarchicalStepDto dto)
    {
        var step = await _hierarchyService.CreateStepAsync(sid, dto);
        return CreatedAtAction(nameof(GetSteps), new { sid }, step);
    }

    [HttpPut("steps/{stepId}")]
    public async Task<ActionResult<HierarchicalStepDto>> UpdateStep([FromRoute] int sid, string stepId, UpdateHierarchicalStepDto dto)
    {
        var step = await _hierarchyService.UpdateStepAsync(sid, stepId, dto);
        return step is null ? NotFound() : Ok(step);
    }

    [HttpDelete("steps/{stepId}")]
    public async Task<IActionResult> DeleteStep([FromRoute] int sid, string stepId)
    {
        await _hierarchyService.DeleteStepAsync(sid, stepId);
        return NoContent();
    }

    /// <summary>
    /// Clears all steps and parameters for a scenario (clears the deck).
    /// </summary>
    [HttpDelete("steps/clear-scenario")]
    public async Task<IActionResult> ClearScenario([FromRoute] int sid)
    {
        await _graph.DeleteAllParameterNodesForScenarioAsync(sid);
        return NoContent();
    }

    [HttpPost("rollup")]
    public async Task<ActionResult<GraphWalkResultTree>> Rollup([FromRoute] int sid)
    {
        try
        {
            var result = await _hierarchyService.RollupAsync(sid);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Rollup failed", details = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
