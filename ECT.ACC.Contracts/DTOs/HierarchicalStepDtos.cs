namespace ECT.ACC.Contracts.DTOs;

public class CreateHierarchicalStepWithParametersDto
{
    // 1. The Step Definition (Metadata)
    public string StepName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? ParentNodeId { get; set; } // For the "Walk" up the tree

    // 2. The Parameter List (The 4 core coefficients: E, T, C, k)
    // Using your existing CreateHierarchicalStepDto for each individual node
    public List<CreateHierarchicalStepDto> Parameters { get; set; } = new();
}

public class CreateHierarchicalStepDto
{
    public string Key { get; set; } = string.Empty;

    // Added to satisfy graph engine mapping
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Role in ECT math (E, T, C, k).
    /// </summary>
    public string Role { get; set; } = "C";

    // Added to satisfy graph engine mapping (maps to Role)
    public string Type { get; set; } = "C";

    /// <summary>
    /// Energy parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? E { get; set; }

    /// <summary>
    /// Control parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? C { get; set; }

    /// <summary>
    /// Complexity parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? K { get; set; }

    /// <summary>
    /// Time parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? T { get; set; }

    /// <summary>
    /// Optional rollup operator for the contributes-to edge from this node to its parent.
    /// </summary>
    public string? RollupOperator { get; set; }

    /// <summary>
    /// Edge weight value for the contributes-to relationship.
    /// </summary>
    public double Weight { get; set; } = 1.0;

    /// <summary>
    /// Parent step node id (if null, attaches to the scenario root node).
    /// </summary>
    public string? ParentNodeId { get; set; }
    public List<string> ParentNodeIds { get; set; } = new();

    /// <summary>
    /// Base value for this step (stored on the USES edge for the scenario).
    /// </summary>
    public ScientificValueDto? BaseValue { get; set; }
}

public class UpdateHierarchicalStepDto
{
    /// <summary>
    /// Updated name/label for the node in the graph.
    /// </summary>
    public string? Name { get; set; }
    public string? Label { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Updated role in ECT math (E, T, C, k).
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Updated functional type in the graph engine.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Updated rollup operator for the parent relationship.
    /// </summary>
    public string? RollupOperator { get; set; }

    /// <summary>
    /// Updated influence/weight of this node on its parent.
    /// </summary>
    public double? Weight { get; set; }

    /// <summary>
    /// Updated base numeric value for the scenario calculation.
    /// </summary>
    public ScientificValueDto? BaseValue { get; set; }

    /// <summary>
    /// Energy parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? E { get; set; }

    /// <summary>
    /// Control parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? C { get; set; }

    /// <summary>
    /// Complexity parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? K { get; set; }

    /// <summary>
    /// Time parameter value for step anchor nodes (Apr 2026).
    /// </summary>
    public ScientificValueDto? T { get; set; }
}

public class HierarchicalStepDto
{
    public string NodeId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ParentNodeId { get; set; }
    public List<string> ParentNodeIds { get; set; } = new();
    public string? RollupOperator { get; set; }
    public double Weight { get; set; } = 1.0;
    // Parameter values stored directly on step anchor nodes (Apr 2026)
    public ScientificValueDto? E { get; set; }
    public ScientificValueDto? C { get; set; }
    public ScientificValueDto? K { get; set; }
    public ScientificValueDto? T { get; set; }
    // Provenance information for each parameter
    public string? EProvenance { get; set; }
    public string? CProvenance { get; set; }
    public string? KProvenance { get; set; }
    public string? TProvenance { get; set; }
    // Retained for backward compatibility with flat scenario consumers
    public ScientificValueDto? BaseValue { get; set; }
}