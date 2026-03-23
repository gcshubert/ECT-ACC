namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// DTO for creating a parameter node in the graph.
/// </summary>
public class CreateParameterNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "C";
    public string? RollupOperator { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating a parameter node in the graph.
/// </summary>
public class UpdateParameterNodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "C";
    public string? RollupOperator { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for parameter node.
/// </summary>
public class ParameterNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RollupOperator { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateEdgeDto
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty; // e.g., "ROLLS_UP_TO", "DEPENDS_ON"
    public string Operation { get; set; } = "Multiply"; // Multiply, Divide, Add, Subtract, Power
}

/// <summary>
/// DTO for updating an edge.
/// </summary>
public class UpdateEdgeDto
{
    public string Relationship { get; set; } = string.Empty;
    public string Operation { get; set; } = "Multiply";

    // New properties for Phase 2 hierarchy
    public double? Weight { get; set; }
    public string? RollupOperator { get; set; }
}
/// <summary>
/// DTO for edge.
/// </summary>
public class EdgeDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public int ScenarioId { get; set; }
}

/// <summary>
/// Represents the scenario->root parameter connection and base values used in a graph walk.
/// </summary>
public class UsesEdgeDto
{
    public string Id { get; set; } = string.Empty;
    public string ScenarioNodeId { get; set; } = string.Empty;
    public string RootParameterNodeId { get; set; } = string.Empty;
    public Dictionary<string, double> BaseParameterValues { get; set; } = new();
}

/// <summary>
/// Represents a CONTRIBUTES_TO edge — used to reconstruct parent/child
/// relationships in the hierarchy tree.
/// </summary>
public class ContributesToEdgeSummaryDto
{
    public string Id { get; set; } = string.Empty; 
    public string ChildId { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public double Weight { get; set; }
    public string? RollupOperator { get; set; }
}