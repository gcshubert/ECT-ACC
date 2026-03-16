using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECT.ACC.Data.Math;

namespace ECT.ACC.Api.Clients;

/// <summary>
/// HTTP wrapper for ECT.Graph.Api.
/// Maps graph service responses to ECT.ACC domain types at the boundary —
/// nothing upstream knows it is talking to a graph service.
///
/// Registered in DI with a named HttpClient so base URL and connection
/// pooling are managed by IHttpClientFactory. Transport can be swapped
/// to gRPC inside this class without touching any call site.
/// </summary>
public class GraphApiClient : IGraphApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GraphApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<GraphWalkResult> GetConfigurationWalkAsync(
        string scenarioGraphId,
        string configurationGraphId)
    {
        var url = $"api/walk/scenario/{scenarioGraphId}/configuration/{configurationGraphId}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content
            .ReadFromJsonAsync<GraphWalkResponseDto>(JsonOptions)
            ?? throw new InvalidOperationException(
                $"Graph walk returned null for scenario {scenarioGraphId}, " +
                $"configuration {configurationGraphId}.");

        return MapToResult(dto);
    }

    // -------------------------------------------------------------------------
    // Mapping — graph service DTOs → ECT.ACC domain types
    // -------------------------------------------------------------------------

    private static GraphWalkResult MapToResult(GraphWalkResponseDto dto) => new(
        Energy: MapValue(dto.Energy),
        Control: MapValue(dto.Control),
        Complexity: MapValue(dto.Complexity),
        TimeAvailable: MapValue(dto.TimeAvailable),
        SolveForMode: dto.SolveForMode,
        Nodes: dto.Nodes.Select(MapNode).ToList());

    private static GraphNodeResult MapNode(GraphNodeDto n) => new(
        NodeId: n.NodeId,
        Name: n.Name,
        Role: n.Role,
        EffectiveValue: n.EffectiveValue is null ? null : MapValue(n.EffectiveValue),
        WeightedContribution: n.WeightedContribution);

    private static ScientificValue MapValue(ScientificValueDto v) =>
        new(v.Coefficient, v.Exponent);

    // -------------------------------------------------------------------------
    // Graph service response DTOs — internal to this class
    // These mirror ECT.Graph.Api's walk response shape.
    // If the graph service contract changes, only this class needs updating.
    // -------------------------------------------------------------------------

    private sealed class GraphWalkResponseDto
    {
        public ScientificValueDto Energy { get; init; } = null!;
        public ScientificValueDto Control { get; init; } = null!;
        public ScientificValueDto Complexity { get; init; } = null!;
        public ScientificValueDto TimeAvailable { get; init; } = null!;
        public string SolveForMode { get; init; } = null!;
        public List<GraphNodeDto> Nodes { get; init; } = [];
    }

    private sealed class GraphNodeDto
    {
        public string NodeId { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string Role { get; init; } = null!;
        public ScientificValueDto? EffectiveValue { get; init; }
        public double? WeightedContribution { get; init; }
    }

    private sealed class ScientificValueDto
    {
        public double Coefficient { get; init; }
        public double Exponent { get; init; }
    }
}