using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECT.ACC.Contracts.DTOs;

namespace ECT.ACC.Api.Services;

/// <summary>
/// Implementation of graph management service that calls ECT.Graph.Api.
/// </summary>
public class GraphManagementService : IGraphManagementService
{
    private readonly HttpClient _httpClient;
    private readonly string _graphApiBaseUrl;

    public GraphManagementService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _graphApiBaseUrl = configuration["GraphApi:BaseUrl"] ?? "http://localhost:50068/api";
    }

    // ── Scenario node helpers ─────────────────────────────────────────────────

    public async Task<string?> GetScenarioGraphIdAsync(int scenarioId)
    {
        var response = await _httpClient.GetAsync($"{_graphApiBaseUrl}/ScenarioNodes/by-external-id/{scenarioId}");
        if (!response.IsSuccessStatusCode) return null;

        var node = await response.Content.ReadFromJsonAsync<ScenarioNodeDto>();
        return node?.Id;
    }

    public async Task<string> EnsureScenarioGraphExistsAsync(
        int scenarioId,
        string name,
        string description,
        string solveForMode,
        string domain)
    {
        var existingId = await GetScenarioGraphIdAsync(scenarioId);
        if (existingId is not null) return existingId;

        var payload = new
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            SolveForMode = solveForMode,
            Domain = domain,
            ExternalScenarioId = scenarioId.ToString(),
        };

        var response = await _httpClient.PostAsJsonAsync($"{_graphApiBaseUrl}/ScenarioNodes", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ScenarioNodeDto>();
        return created!.Id;
    }

    // ── Hierarchical graph nodes + edges ───────────────────────────────────────

    public async Task<ParameterNodeDto> CreateParameterNodeAsync(int scenarioId, CreateParameterNodeDto dto)
    {
        var graphNode = new
        {
            Id = dto.Id,
            Name = dto.Name,
            Role = dto.Role,
            RollupOperator = dto.RollupOperator ?? string.Empty,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var response = await _httpClient.PostAsJsonAsync($"{_graphApiBaseUrl}/ParameterNodes", graphNode);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ParameterNodeDto>();
        return created!;
    }

    public async Task<ParameterNodeDto?> GetParameterNodeAsync(int scenarioId, string nodeId)
    {
        var response = await _httpClient.GetAsync($"{_graphApiBaseUrl}/ParameterNodes/{nodeId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ParameterNodeDto>();
    }

    public async Task<IEnumerable<ParameterNodeDto>> GetParameterNodesAsync(int scenarioId)
    {
        var response = await _httpClient.GetAsync($"{_graphApiBaseUrl}/ParameterNodes");
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<ParameterNodeDto>();

        var nodes = await response.Content
            .ReadFromJsonAsync<IEnumerable<ParameterNodeDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });

        return nodes ?? Enumerable.Empty<ParameterNodeDto>();
    }

    public async Task<IEnumerable<ContributesToEdgeSummaryDto>> GetContributesToEdgesAsync()
    {
        var response = await _httpClient.GetAsync($"{_graphApiBaseUrl}/Edges/contributes-to");
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<ContributesToEdgeSummaryDto>();

        var edges = await response.Content
            .ReadFromJsonAsync<IEnumerable<ContributesToEdgeSummaryDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return edges ?? Enumerable.Empty<ContributesToEdgeSummaryDto>();
    }

    public async Task<ParameterNodeDto?> UpdateParameterNodeAsync(int scenarioId, string nodeId, UpdateParameterNodeDto dto)
    {
        var updatePayload = new
        {
            Name = dto.Name,
            Role = dto.Role,
            RollupOperator = dto.RollupOperator ?? string.Empty,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var response = await _httpClient.PutAsJsonAsync($"{_graphApiBaseUrl}/ParameterNodes/{nodeId}", updatePayload);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ParameterNodeDto>();
    }

    public async Task<bool> DeleteParameterNodeAsync(int scenarioId, string nodeId)
    {
        var response = await _httpClient.DeleteAsync($"{_graphApiBaseUrl}/ParameterNodes/{nodeId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<EdgeDto> CreateEdgeAsync(int scenarioId, CreateEdgeDto dto)
    {
        var edge = new
        {
            Id = Guid.NewGuid().ToString(),
            FromParameterNodeId = dto.SourceNodeId,
            ToParameterNodeId = dto.TargetNodeId,
            RollupOperator = dto.Operation,
            Weight = 1.0
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_graphApiBaseUrl}/Edges/contributes-to", edge);
        response.EnsureSuccessStatusCode();

        // EdgesController returns ContributesToEdge, not EdgeDto — map it
        var created = await response.Content.ReadFromJsonAsync<ContributesToEdgeResponse>();
        return new EdgeDto
        {
            Id = created!.Id,
            SourceNodeId = created.FromParameterNodeId,
            TargetNodeId = created.ToParameterNodeId,
            Relationship = "CONTRIBUTES_TO",
            Operation = created.RollupOperator,
            ScenarioId = scenarioId
        };
    }

    // Private response shape matching ContributesToEdge from ECT.Graph.Api
    private sealed class ContributesToEdgeResponse
    {
        public string Id { get; init; } = string.Empty;
        public string FromParameterNodeId { get; init; } = string.Empty;
        public string ToParameterNodeId { get; init; } = string.Empty;
        public string RollupOperator { get; init; } = string.Empty;
        public double Weight { get; init; }
    }
    public async Task<IEnumerable<EdgeDto>> GetEdgesAsync(int scenarioId)
    {
        var edges = await _httpClient.GetFromJsonAsync<IEnumerable<EdgeDto>>($"{_graphApiBaseUrl}/Edges");
        return edges?.Where(e => e.ScenarioId == scenarioId) ?? Enumerable.Empty<EdgeDto>();
    }

    public async Task<EdgeDto?> UpdateEdgeAsync(int scenarioId, string edgeId, UpdateEdgeDto dto)
    {
        var updatePayload = new
        {
            Relationship = dto.Relationship,
            Operation = dto.Operation
        };

        var response = await _httpClient.PutAsJsonAsync($"{_graphApiBaseUrl}/Edges/{edgeId}", updatePayload);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<EdgeDto>();
    }

    public async Task UpdateEdgePropertiesAsync(int scenarioId, string stepId, UpdateEdgeDto dto)
    {
        // 1. Find the existing CONTRIBUTES_TO edge where this node is the child
        var allEdges = await GetContributesToEdgesAsync();
        var existing = allEdges.FirstOrDefault(e => e.ChildId == stepId);

        if (existing is null)
            throw new InvalidOperationException(
                $"No CONTRIBUTES_TO edge found for node {stepId} in scenario {scenarioId}.");

        // 2. Delete the existing edge
        await _httpClient.DeleteAsync($"{_graphApiBaseUrl}/Edges/contributes-to/{existing.Id}");
        // 3. Recreate with updated properties
        var updated = new
        {
            Id = Guid.NewGuid().ToString(),
            FromParameterNodeId = stepId,
            ToParameterNodeId = existing.ParentId,
            RollupOperator = dto.RollupOperator ?? existing.RollupOperator,
            Weight = dto.Weight ?? existing.Weight
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_graphApiBaseUrl}/Edges/contributes-to", updated);

        response.EnsureSuccessStatusCode();
    }
    public async Task<bool> DeleteEdgeAsync(int scenarioId, string edgeId)
    {
        var response = await _httpClient.DeleteAsync($"{_graphApiBaseUrl}/Edges/{edgeId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<UsesEdgeDto?> GetUsesEdgeAsync(int scenarioId)
    {
        var scenarioGraphId = await GetScenarioGraphIdAsync(scenarioId);
        if (scenarioGraphId is null) return null;

        var response = await _httpClient.GetAsync($"{_graphApiBaseUrl}/Edges/uses/by-scenario/{scenarioGraphId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UsesEdgeDto>()
            ?? throw new InvalidOperationException("Graph API returned null for UsesEdge.");
    }

    public async Task<UsesEdgeDto> UpsertUsesEdgeAsync(int scenarioId, UsesEdgeDto dto)
    {
        var scenarioGraphId = await GetScenarioGraphIdAsync(scenarioId);
        if (scenarioGraphId is null)
        {
            throw new InvalidOperationException($"Scenario graph node not found for scenario {scenarioId}");
        }

        var payload = new
        {
            RootParameterNodeId = dto.RootParameterNodeId,
            BaseParameterValues = dto.BaseParameterValues
        };

        var response = await _httpClient.PutAsJsonAsync($"{_graphApiBaseUrl}/Edges/uses/{scenarioGraphId}", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UsesEdgeDto>()
            ?? throw new InvalidOperationException("Graph API returned null for UsesEdge.");
    }
}