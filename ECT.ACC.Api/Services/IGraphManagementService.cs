using ECT.ACC.Contracts.DTOs;

namespace ECT.ACC.Api.Services;

/// <summary>
/// Service for managing graph data (nodes and edges) for scenarios.
/// Integrates with ECT.Graph.Api.
/// </summary>
public interface IGraphManagementService
{
    // Scenario graph node handling
    Task<string?> GetScenarioGraphIdAsync(int scenarioId);
    Task<string> EnsureScenarioGraphExistsAsync(int scenarioId, string name, string description, string solveForMode, string domain);

    // Hierarchical parameter topology
    Task<ParameterNodeDto> CreateParameterNodeAsync(int scenarioId, CreateParameterNodeDto dto);
    Task<ParameterNodeDto?> GetParameterNodeAsync(int scenarioId, string nodeId);
    Task<IEnumerable<ParameterNodeDto>> GetParameterNodesAsync(int scenarioId);
    Task<ParameterNodeDto?> UpdateParameterNodeAsync(int scenarioId, string nodeId, UpdateParameterNodeDto dto);
    Task<bool> DeleteParameterNodeAsync(int scenarioId, string nodeId);

    // Edges
    Task<EdgeDto> CreateEdgeAsync(int scenarioId, CreateEdgeDto dto);
    Task<IEnumerable<EdgeDto>> GetEdgesAsync(int scenarioId);
    Task UpdateEdgePropertiesAsync(int scenarioId, string stepId, UpdateEdgeDto dto);
    Task<EdgeDto?> UpdateEdgeAsync(int scenarioId, string edgeId, UpdateEdgeDto dto);
    Task<bool> DeleteEdgeAsync(int scenarioId, string edgeId);

    // Uses edge (base values)
    Task<UsesEdgeDto?> GetUsesEdgeAsync(int scenarioId);
    Task<UsesEdgeDto> UpsertUsesEdgeAsync(int scenarioId, UsesEdgeDto dto);
}