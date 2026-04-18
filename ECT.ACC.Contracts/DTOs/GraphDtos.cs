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

    // Scientific value for leaf nodes (maintains precision)
    public ScientificValueDto? ScientificValue { get; set; }

    // Parameter values stored directly on step anchor nodes (Apr 2026)
    public ScientificValueDto? E { get; set; }
    public ScientificValueDto? C { get; set; }
    public ScientificValueDto? K { get; set; }
    public ScientificValueDto? T { get; set; }
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

    // Scientific value properties for Graph.Api compatibility
    public double? Coefficient { get; set; }
    public double? Exponent { get; set; }
    public string? Provenance { get; set; }
    public string? ExternalScenarioId { get; set; }

    // Parameter properties for step anchor nodes (Apr 2026)
    public double? ECoefficient { get; set; }
    public double? EExponent { get; set; }
    public double? CCoefficient { get; set; }
    public double? CExponent { get; set; }
    public double? KCoefficient { get; set; }
    public double? KExponent { get; set; }
    public double? TCoefficient { get; set; }
    public double? TExponent { get; set; }
    public string? EProvenance { get; set; }
    public string? CProvenance { get; set; }
    public string? KProvenance { get; set; }
    public string? TProvenance { get; set; }
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

    // Scientific value for leaf nodes (maintains precision)
    public ScientificValueDto? ScientificValue { get; set; }

    // Parameter values stored directly on step anchor nodes (Apr 2026)
    public ScientificValueDto? E { get; set; }
    public ScientificValueDto? C { get; set; }
    public ScientificValueDto? K { get; set; }
    public ScientificValueDto? T { get; set; }
}

public class CreateEdgeDto
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Operation { get; set; } = "Multiply";
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// DTO for updating an edge.
/// </summary>
public class UpdateEdgeDto
{
    public string Relationship { get; set; } = string.Empty;
    public string Operation { get; set; } = "Multiply";
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
    public Dictionary<string, ScientificValueDto> BaseParameterValues { get; set; } = new();
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
    public int SortOrder { get; set; } = 0;
}
