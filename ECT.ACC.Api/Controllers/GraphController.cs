using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECT.ACC.Api.Controllers;

/// <summary>
/// Manages graph data (nodes and edges) for hierarchical scenarios.
/// Calls ECT.Graph.Api for actual graph operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GraphController : ControllerBase
{
    private readonly IGraphManagementService _graphService;

    public GraphController(IGraphManagementService graphService)
    {
        _graphService = graphService;
    }

    /// <summary>
    /// Creates a parameter node for a scenario.
    /// </summary>
    [HttpPost("scenario/{scenarioId}/nodes")]
    public async Task<ActionResult<ParameterNodeDto>> CreateNode(int scenarioId, [FromBody] CreateParameterNodeDto dto)
    {
        var node = await _graphService.CreateParameterNodeAsync(scenarioId, dto);
        return CreatedAtAction(nameof(GetNodes), new { scenarioId }, node);
    }

    /// <summary>
    /// Gets all parameter nodes for a scenario.
    /// </summary>
    [HttpGet("scenario/{scenarioId}/nodes")]
    public async Task<ActionResult<IEnumerable<ParameterNodeDto>>> GetNodes(int scenarioId)
    {
        var nodes = await _graphService.GetParameterNodesAsync(scenarioId);
        return Ok(nodes);
    }

    /// <summary>
    /// Updates a parameter node.
    /// </summary>
    [HttpPut("scenario/{scenarioId}/nodes/{nodeId}")]
    public async Task<ActionResult<ParameterNodeDto>> UpdateNode(int scenarioId, string nodeId, [FromBody] UpdateParameterNodeDto dto)
    {
        var node = await _graphService.UpdateParameterNodeAsync(scenarioId, nodeId, dto);
        return node is null ? NotFound() : Ok(node);
    }

    /// <summary>
    /// Deletes a parameter node.
    /// </summary>
    [HttpDelete("scenario/{scenarioId}/nodes/{nodeId}")]
    public async Task<IActionResult> DeleteNode(int scenarioId, string nodeId)
    {
        var deleted = await _graphService.DeleteParameterNodeAsync(scenarioId, nodeId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Creates an edge between nodes for a scenario.
    /// </summary>
    [HttpPost("scenario/{scenarioId}/edges")]
    public async Task<ActionResult<EdgeDto>> CreateEdge(int scenarioId, [FromBody] CreateEdgeDto dto)
    {
        var edge = await _graphService.CreateEdgeAsync(scenarioId, dto);
        return CreatedAtAction(nameof(GetEdges), new { scenarioId }, edge);
    }

    /// <summary>
    /// Gets all edges for a scenario.
    /// </summary>
    [HttpGet("scenario/{scenarioId}/edges")]
    public async Task<ActionResult<IEnumerable<EdgeDto>>> GetEdges(int scenarioId)
    {
        var edges = await _graphService.GetEdgesAsync(scenarioId);
        return Ok(edges);
    }

    /// <summary>
    /// Updates an edge.
    /// </summary>
    [HttpPut("scenario/{scenarioId}/edges/{edgeId}")]
    public async Task<ActionResult<EdgeDto>> UpdateEdge(int scenarioId, string edgeId, [FromBody] UpdateEdgeDto dto)
    {
        var edge = await _graphService.UpdateEdgeAsync(scenarioId, edgeId, dto);
        return edge is null ? NotFound() : Ok(edge);
    }

    /// <summary>
    /// Deletes an edge.
    /// </summary>
    [HttpDelete("scenario/{scenarioId}/edges/{edgeId}")]
    public async Task<IActionResult> DeleteEdge(int scenarioId, string edgeId)
    {
        var deleted = await _graphService.DeleteEdgeAsync(scenarioId, edgeId);
        return deleted ? NoContent() : NotFound();
    }
}