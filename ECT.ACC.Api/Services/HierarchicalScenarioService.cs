using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Math;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Services;

public interface IHierarchicalScenarioService
{
    Task<HierarchicalStepDto> CreateStepAsync(int scenarioId, CreateHierarchicalStepDto dto);
    Task<IEnumerable<HierarchicalStepDto>> GetStepsAsync(int scenarioId);
    Task<HierarchicalStepDto?> UpdateStepAsync(int scenarioId, string stepId, UpdateHierarchicalStepDto dto);
    Task<bool> DeleteStepAsync(int scenarioId, string stepId);
    Task<GraphWalkResultTree> RollupAsync(int scenarioId);
}

public class HierarchicalScenarioService : IHierarchicalScenarioService
{
    private readonly ECTDbContext _context;
    private readonly IGraphManagementService _graph;
    private readonly IGraphApiClient _graphClient;

    public HierarchicalScenarioService(
        ECTDbContext context,
        IGraphManagementService graph,
        IGraphApiClient graphClient)
    {
        _context = context;
        _graph = graph;
        _graphClient = graphClient;
    }

    private static string RootNodeId(int scenarioId) => $"scenario-{scenarioId}-root";

    private async Task<string> EnsureRootAndUsesEdgeAsync(int scenarioId)
    {
        var scenario = await _context.Scenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == scenarioId);

        if (scenario is null)
            throw new InvalidOperationException($"Scenario {scenarioId} not found.");

        var scenarioGraphId = await _graph.EnsureScenarioGraphExistsAsync(
            scenarioId,
            scenario.Name,
            scenario.Description,
            scenario.SolveForMode,
            scenario.ProcessDomainId?.ToString() ?? string.Empty);

        var rootId = RootNodeId(scenarioId);
        var existingRootNode = await _graph.GetParameterNodeAsync(scenarioId, rootId);
        if (existingRootNode is null)
        {
            await _graph.CreateParameterNodeAsync(scenarioId, new CreateParameterNodeDto
            {
                Id = rootId,
                Name = "Scenario Root",
                Description = "Root of the hierarchical parameter topology",
                Role = "k",
                RollupOperator = "Sum",
                IsActive = true
            });
        }

        var usesEdge = await _graph.GetUsesEdgeAsync(scenarioId);
        if (usesEdge is null)
        {
            await _graph.UpsertUsesEdgeAsync(scenarioId, new UsesEdgeDto
            {
                ScenarioNodeId = scenarioGraphId,
                RootParameterNodeId = rootId,
                BaseParameterValues = new Dictionary<string, double>()
            });
        }

        return rootId;
    }

    public async Task<HierarchicalStepDto> CreateStepAsync(int scenarioId, CreateHierarchicalStepDto dto)
    {
        var rootId = await EnsureRootAndUsesEdgeAsync(scenarioId);
        var stepId = string.IsNullOrWhiteSpace(dto.Key) ? Guid.NewGuid().ToString() : dto.Key;

        var node = await _graph.CreateParameterNodeAsync(scenarioId, new CreateParameterNodeDto
        {
            Id = stepId,
            Name = dto.Label,
            Description = dto.Description,
            Role = dto.Role,
            RollupOperator = dto.RollupOperator,
            IsActive = true
        });

        var parentId = string.IsNullOrWhiteSpace(dto.ParentNodeId) ? rootId : dto.ParentNodeId;
        var maxSortOrder = await _graph.GetMaxSortOrderForParentAsync(parentId);
        await _graph.CreateEdgeAsync(scenarioId, new CreateEdgeDto
        {
            SourceNodeId = node.Id,
            TargetNodeId = parentId,
            Relationship = "CONTRIBUTES_TO",
            Operation = dto.RollupOperator ?? "WeightedSum",
            SortOrder = maxSortOrder + 1
        });

        if (dto.BaseValue.HasValue)
        {
            var uses = await _graph.GetUsesEdgeAsync(scenarioId);
            var baseValues = uses?.BaseParameterValues ?? new Dictionary<string, double>();
            baseValues[node.Id] = dto.BaseValue.Value;
            await _graph.UpsertUsesEdgeAsync(scenarioId, new UsesEdgeDto
            {
                ScenarioNodeId = uses?.ScenarioNodeId ?? await _graph.GetScenarioGraphIdAsync(scenarioId) ?? string.Empty,
                RootParameterNodeId = rootId,
                BaseParameterValues = baseValues
            });
        }

        // Build parentIds list for DAG support
        var parentIds = parentId == rootId
            ? new List<string>()
            : new List<string> { parentId };

        return new HierarchicalStepDto
        {
            NodeId = node.Id,
            Key = stepId,
            Label = node.Name,
            Description = node.Description,
            Role = dto.Role,
            ParentNodeIds = parentIds,
            ParentNodeId = parentIds.FirstOrDefault(),
            RollupOperator = dto.RollupOperator,
            Weight = dto.Weight,
            BaseValue = dto.BaseValue
        };
    }

    public async Task<IEnumerable<HierarchicalStepDto>> GetStepsAsync(int scenarioId)
    {
        var uses = await _graph.GetUsesEdgeAsync(scenarioId);
        if (uses is null) return Enumerable.Empty<HierarchicalStepDto>();

        var allNodes = await _graph.GetParameterNodesAsync(scenarioId);
        var allEdges = await _graph.GetContributesToEdgesAsync();

        var parentLookup = allEdges
            .GroupBy(e => e.ChildId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ParentId).ToList());

        var rootId = RootNodeId(scenarioId);
        var stepNodes = allNodes.Where(n => n.Id != rootId).ToList();

        return stepNodes.Select(node =>
        {
            // Declared outside the initializer — this was the root cause
            var parentIds = parentLookup.TryGetValue(node.Id, out var ids)
                ? ids.Where(id => id != rootId).ToList()
                : new List<string>();

            return new HierarchicalStepDto
            {
                NodeId = node.Id,
                Key = node.Id,
                Label = node.Name,
                Description = node.Description,
                Role = node.Role,
                ParentNodeIds = parentIds,
                ParentNodeId = parentIds.FirstOrDefault(),
                RollupOperator = allEdges.FirstOrDefault(e => e.ChildId == node.Id)?.RollupOperator,
                Weight = allEdges.FirstOrDefault(e => e.ChildId == node.Id)?.Weight ?? 1.0,
                BaseValue = uses.BaseParameterValues.TryGetValue(node.Id, out var val) ? val : null
            };
        });
    }

    public async Task<HierarchicalStepDto?> UpdateStepAsync(int scenarioId, string stepId, UpdateHierarchicalStepDto dto)
    {
        var node = await _graph.GetParameterNodeAsync(scenarioId, stepId);
        if (node is null) return null;

        // 1. Update Core Node Properties
        // Using the nullable properties to allow partial updates
        var updated = await _graph.UpdateParameterNodeAsync(scenarioId, stepId, new UpdateParameterNodeDto
        {
            // Support both Name/Label and Type/Role naming for engine parity
            Name = dto.Label ?? dto.Name ?? node.Name,
            Description = dto.Description ?? node.Description,
            Role = dto.Role ?? dto.Type ?? node.Role,
            RollupOperator = dto.RollupOperator ?? node.RollupOperator,
            IsActive = true
        });

        // 2. Persist Weight/Edge Properties (New for Hierarchy)
        // This addresses the logic previously missing from the flat model
        if (dto.Weight.HasValue || dto.RollupOperator != null)
        {
            await _graph.UpdateEdgePropertiesAsync(scenarioId, stepId, new UpdateEdgeDto
            {
                Weight = dto.Weight,
                RollupOperator = dto.RollupOperator
            });
        }

        // 3. Handle BaseValue (Existing Logic)
        if (dto.BaseValue.HasValue)
        {
            var uses = await _graph.GetUsesEdgeAsync(scenarioId);
            var baseValues = uses?.BaseParameterValues ?? new Dictionary<string, double>();
            baseValues[stepId] = dto.BaseValue.Value;

            await _graph.UpsertUsesEdgeAsync(scenarioId, new UsesEdgeDto
            {
                ScenarioNodeId = uses?.ScenarioNodeId ?? await _graph.GetScenarioGraphIdAsync(scenarioId) ?? string.Empty,
                RootParameterNodeId = await EnsureRootAndUsesEdgeAsync(scenarioId),
                BaseParameterValues = baseValues
            });
        }

        // 4. Return Refreshed DTO
        // Use the local variables and dto values to avoid CS1061
        return new HierarchicalStepDto
        {
            NodeId = stepId,
            Key = stepId,
            Name = updated?.Name ?? node.Name,
            Label = updated?.Name ?? node.Name,
            Description = updated?.Description ?? node.Description,
            Role = updated?.Role ?? node.Role,
            Type = updated?.Role ?? node.Role,
            Weight = dto.Weight ?? 1.0,

            // Fix: Pull from the DTO directly or keep it null if not updated
            BaseValue = dto.BaseValue
        };
    }

    public async Task<bool> DeleteStepAsync(int scenarioId, string stepId)
    {
        var uses = await _graph.GetUsesEdgeAsync(scenarioId);
        if (uses is not null && uses.BaseParameterValues.ContainsKey(stepId))
        {
            uses.BaseParameterValues.Remove(stepId);
            await _graph.UpsertUsesEdgeAsync(scenarioId, uses);
        }

        return await _graph.DeleteParameterNodeAsync(scenarioId, stepId);
    }

    public async Task<GraphWalkResultTree> RollupAsync(int scenarioId)
    {
        var scenarioGraphId = await _graph.GetScenarioGraphIdAsync(scenarioId);
        if (scenarioGraphId is null)
            throw new InvalidOperationException($"Scenario graph node not found for scenario {scenarioId}");

        return await _graphClient.GetConfigurationWalkTreeAsync(scenarioGraphId, scenarioGraphId);
    }

    private static void SetBaseValue(Dictionary<string, double> dict, string nodeId, ScientificValueDto value)
    {
        dict[$"{nodeId}:coeff"] = value.Coefficient;
        dict[$"{nodeId}:exp"] = value.Exponent;
    }

    private static ScientificValueDto? GetBaseValue(Dictionary<string, double> dict, string nodeId)
    {
        if (dict.TryGetValue($"{nodeId}:coeff", out var coeff) &&
            dict.TryGetValue($"{nodeId}:exp", out var exp))
            return new ScientificValueDto { Coefficient = coeff, Exponent = exp };
        return null;
    }
}
