using ECT.ACC.Data.Math;

namespace ECT.ACC.Api.Clients;

/// <summary>
/// Contract for delegating analytical work to ECT.Graph.Api.
/// ECT.ACC.Api calls this — the graph service is invisible to the UI.
/// </summary>
public interface IGraphApiClient
{
    /// <summary>
    /// Executes a configuration-level graph walk and returns the rolled-up
    /// ECT core parameter values for the given solve-for mode.
    /// </summary>
    Task<GraphWalkResult> GetConfigurationWalkAsync(
        string scenarioGraphId,
        string configurationGraphId);
}

/// <summary>
/// Rolled-up ECT core parameter values returned from a graph walk.
/// All four variables are always populated from the walk — which ones
/// are treated as inputs vs compute targets depends on solveForMode.
/// </summary>
public record GraphWalkResult(
    ScientificValue Energy,
    ScientificValue Control,
    ScientificValue Complexity,
    ScientificValue TimeAvailable,
    string SolveForMode,
    IReadOnlyList<GraphNodeResult> Nodes);

/// <summary>
/// Per-node result from the graph walk — used for diagnostic tree
/// output and bottleneck identification (Track 3).
/// </summary>
public record GraphNodeResult(
    string NodeId,
    string Name,
    string Role,
    ScientificValue? EffectiveValue,
    double? WeightedContribution);