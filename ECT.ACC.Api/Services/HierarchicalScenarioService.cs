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

    /// <summary>
    /// Ensures the scenario graph node and root ParameterNode exist.
    /// </summary>
    private async Task EnsureRootNodeExistsAsync(int scenarioId)
    {
        var scenario = await _context.Scenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == scenarioId);

        if (scenario is null)
            throw new InvalidOperationException($"Scenario {scenarioId} not found.");

        await _graph.EnsureScenarioGraphExistsAsync(
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
    }

    public async Task<HierarchicalStepDto> CreateStepAsync(int scenarioId, CreateHierarchicalStepDto dto)
    {
        await EnsureRootNodeExistsAsync(scenarioId);

        var rootId = RootNodeId(scenarioId);
        var stepId = string.IsNullOrWhiteSpace(dto.Key) ? Guid.NewGuid().ToString() : dto.Key;

        // Create step anchor node with E/C/K/T stored as properties on the node
        var node = await _graph.CreateParameterNodeAsync(scenarioId, new CreateParameterNodeDto
        {
            Id = stepId,
            Name = dto.Label,
            Description = dto.Description,
            Role = "k",
            RollupOperator = dto.RollupOperator,
            IsActive = true,
            E = dto.E,
            C = dto.C,
            K = dto.K,
            T = dto.T
        });

        // Wire step anchor to parent via CONTRIBUTES_TO edge
        var parentId = string.IsNullOrWhiteSpace(dto.ParentNodeId) ? rootId : dto.ParentNodeId;
        var maxSortOrder = await _graph.GetMaxSortOrderForParentAsync(parentId);
        await _graph.CreateEdgeAsync(scenarioId, new CreateEdgeDto
        {
            SourceNodeId = node.Id,
            TargetNodeId = parentId,
            Relationship = "CONTRIBUTES_TO",
            Operation = dto.RollupOperator ?? "Sum",
            SortOrder = maxSortOrder + 1
        });

        var parentIds = parentId == rootId
            ? new List<string>()
            : new List<string> { parentId };

        return new HierarchicalStepDto
        {
            NodeId = node.Id,
            Key = stepId,
            Name = node.Name,
            Label = node.Name,
            Description = node.Description,
            Role = "k",
            ParentNodeIds = parentIds,
            ParentNodeId = parentIds.FirstOrDefault(),
            RollupOperator = dto.RollupOperator,
            Weight = dto.Weight,
            E = dto.E,
            C = dto.C,
            K = dto.K,
            T = dto.T
        };
    }

    public async Task<IEnumerable<HierarchicalStepDto>> GetStepsAsync(int scenarioId)
    {
        await EnsureRootNodeExistsAsync(scenarioId);

        var rootId = RootNodeId(scenarioId);
        var allNodes = (await _graph.GetAllNodesForScenarioAsync(scenarioId)).ToList();
        var allEdges = (await _graph.GetAllContributesToEdgesAsync()).ToList();

        // Anchor nodes: role=k, not the scenario root
        var anchorNodes = allNodes
            .Where(n => n.Id != rootId && n.Role == "k")
            .ToList();

        // Build parent lookup from edges
        var parentLookup = allEdges
            .GroupBy(e => e.ChildId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ParentId).ToList());

        return anchorNodes.Select(anchor =>
        {
            // Debug what Graph API returns
            Console.WriteLine($"Graph API returned for node {anchor.Id}:");
            Console.WriteLine($"  ECoefficient: {anchor.ECoefficient}");
            Console.WriteLine($"  EExponent: {anchor.EExponent}");
            Console.WriteLine($"  CCoefficient: {anchor.CCoefficient}");
            Console.WriteLine($"  CExponent: {anchor.CExponent}");
            Console.WriteLine($"  KCoefficient: {anchor.KCoefficient}");
            Console.WriteLine($"  KExponent: {anchor.KExponent}");
            Console.WriteLine($"  TCoefficient: {anchor.TCoefficient}");
            Console.WriteLine($"  TExponent: {anchor.TExponent}");

            var e = anchor.ECoefficient.HasValue && anchor.EExponent.HasValue
                ? new ScientificValueDto { Coefficient = anchor.ECoefficient.Value, Exponent = anchor.EExponent.Value }
                : null;
            var c = anchor.CCoefficient.HasValue && anchor.CExponent.HasValue
                ? new ScientificValueDto { Coefficient = anchor.CCoefficient.Value, Exponent = anchor.CExponent.Value }
                : null;
            var k = anchor.KCoefficient.HasValue && anchor.KExponent.HasValue
                ? new ScientificValueDto { Coefficient = anchor.KCoefficient.Value, Exponent = anchor.KExponent.Value }
                : null;
            var t = anchor.TCoefficient.HasValue && anchor.TExponent.HasValue
                ? new ScientificValueDto { Coefficient = anchor.TCoefficient.Value, Exponent = anchor.TExponent.Value }
                : null;

            var parentIds = parentLookup.TryGetValue(anchor.Id, out var ids)
                ? ids.Where(id => id != rootId).ToList()
                : new List<string>();

            var edge = allEdges.FirstOrDefault(e => e.ChildId == anchor.Id);

            var result = new HierarchicalStepDto
            {
                NodeId = anchor.Id,
                Key = anchor.Id,
                Name = anchor.Name,
                Label = anchor.Name,
                Description = anchor.Description,
                Role = anchor.Role,
                ParentNodeIds = parentIds,
                ParentNodeId = parentIds.FirstOrDefault(),
                RollupOperator = edge?.RollupOperator,
                Weight = edge?.Weight ?? 1.0,
                E = e,
                C = c,
                K = k,
                T = t,
                EProvenance = anchor.EProvenance,
                CProvenance = anchor.CProvenance,
                KProvenance = anchor.KProvenance,
                TProvenance = anchor.TProvenance
            };

            // Debug what backend returns to frontend
            Console.WriteLine($"Backend returning to frontend for node {anchor.Id}:");
            Console.WriteLine($"  E: {e?.Coefficient}×10^{e?.Exponent}");
            Console.WriteLine($"  C: {c?.Coefficient}×10^{c?.Exponent}");
            Console.WriteLine($"  K: {k?.Coefficient}×10^{k?.Exponent}");
            Console.WriteLine($"  T: {t?.Coefficient}×10^{t?.Exponent}");

            return result;
        });
    }

    public async Task<HierarchicalStepDto?> UpdateStepAsync(int scenarioId, string stepId, UpdateHierarchicalStepDto dto)
    {
        var node = await _graph.GetParameterNodeAsync(scenarioId, stepId);
        if (node is null) return null;

        var updated = await _graph.UpdateParameterNodeAsync(scenarioId, stepId, new UpdateParameterNodeDto
        {
            Name = dto.Label ?? dto.Name ?? node.Name,
            Description = dto.Description ?? node.Description,
            Role = node.Role,
            RollupOperator = dto.RollupOperator ?? node.RollupOperator,
            IsActive = true,
            E = dto.E,
            C = dto.C,
            K = dto.K,
            T = dto.T
        });

        // Update edge weight/operator if provided
        if (dto.Weight.HasValue || dto.RollupOperator != null)
        {
            await _graph.UpdateEdgePropertiesAsync(scenarioId, stepId, new UpdateEdgeDto
            {
                Weight = dto.Weight,
                RollupOperator = dto.RollupOperator
            });
        }

        return new HierarchicalStepDto
        {
            NodeId = stepId,
            Key = stepId,
            Name = updated?.Name ?? node.Name,
            Label = updated?.Name ?? node.Name,
            Description = updated?.Description ?? node.Description,
            Role = node.Role,
            RollupOperator = dto.RollupOperator ?? node.RollupOperator,
            Weight = dto.Weight ?? 1.0,
            E = dto.E,
            C = dto.C,
            K = dto.K,
            T = dto.T
        };
    }

    public async Task<bool> DeleteStepAsync(int scenarioId, string stepId)
    {
        return await _graphClient.DeleteStepWithLeavesAsync(stepId);
    }

    public async Task<GraphWalkResultTree> RollupAsync(int scenarioId)
    {
        var scenarioGraphId = await _graph.GetScenarioGraphIdAsync(scenarioId);
        if (scenarioGraphId is null)
            throw new InvalidOperationException(
                $"Scenario graph node not found for scenario {scenarioId}");

        return await _graphClient.GetScenarioWalkTreeAsync(scenarioGraphId);
    }
}
