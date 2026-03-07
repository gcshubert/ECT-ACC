using System.ComponentModel.DataAnnotations;
using ECT.ACC.Data.Math;

namespace ECT.ACC.Data.Models;

/// <summary>
/// Stores the human-readable derivation narrative and ordered sub-steps
/// that explain how a top-level ECT parameter value (E, C, k, T) was
/// arrived at.  One ParameterDocumentation row per parameter per Scenario.
/// </summary>
public class ParameterDocumentation
{
    public int Id { get; set; }

    // ── Foreign keys ──────────────────────────────────────────────────────────
    public int ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = null!;

    /// <summary>
    /// Which ECT parameter this documents: "e", "c", "k", or "t".
    /// Matches the keys used in EctParameters.
    /// </summary>
    [MaxLength(8)]
    public string ParameterKey { get; set; } = string.Empty;

    // ── Display ───────────────────────────────────────────────────────────────
    /// <summary>Human-readable label, e.g. "k — Complexity Constant"</summary>
    [MaxLength(120)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Prose explanation of what this parameter represents and why
    /// the final value is what it is.  Mirrors the "Basis" column in
    /// the LUCA paper parameter table.
    /// </summary>
    public string DerivationNarrative { get; set; } = string.Empty;

    // ── Navigation ────────────────────────────────────────────────────────────
    public ICollection<SubParameter> SubParameters { get; set; } = new List<SubParameter>();
    public ICollection<ParameterVariant> Variants { get; set; } = new List<ParameterVariant>();
}

/// <summary>
/// One step in the derivation chain for a parameter.
/// Steps are ordered; each represents an intermediate value or
/// reasoning step that composes into the parent ScientificValue.
/// </summary>
public class SubParameter
{
    public int Id { get; set; }

    // ── Foreign key ───────────────────────────────────────────────────────────
    public int ParameterDocumentationId { get; set; }
    public ParameterDocumentation ParameterDocumentation { get; set; } = null!;

    /// <summary>1-based ordering within the derivation chain.</summary>
    public int StepOrder { get; set; }

    /// <summary>Short name for this step, e.g. "Sequence search space"</summary>
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The numerical value for this step, stored as coefficient × 10^exponent.
    /// </summary>
    public ScientificValueOwned Value { get; set; } = new();

    /// <summary>Unit string, e.g. "bp", "yr", "dimensionless"</summary>
    [MaxLength(60)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Prose justification for the value at this step.</summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>BibTeX key, DOI, or free-text citation for the primary source.</summary>
    [MaxLength(300)]
    public string SourceReference { get; set; } = string.Empty;

    /// <summary>
    /// Phase 3.5 — arithmetic operation this step applies to the running accumulator.
    /// Defaults to Multiply, preserving existing behaviour for all current scenarios.
    /// </summary>
    public StepOperation Operation { get; set; } = StepOperation.Multiply;
}

/// <summary>
/// A named configuration of SubParameter overrides that can be swapped
/// in as a unit.  E.g. "Baseline" vs "Fiber Laser Upgrade".
/// </summary>
public class ParameterVariant
{
    public int Id { get; set; }

    // ── Foreign key ───────────────────────────────────────────────────────────
    public int ParameterDocumentationId { get; set; }
    public ParameterDocumentation ParameterDocumentation { get; set; } = null!;

    /// <summary>Display name, e.g. "Baseline", "Fiber Laser Upgrade"</summary>
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this is the currently active variant for the scenario.</summary>
    public bool IsActive { get; set; }

    public ICollection<VariantSubParameter> SubParameters { get; set; } = new List<VariantSubParameter>();
}

/// <summary>
/// A sub-parameter value within a specific ParameterVariant.
/// Mirrors SubParameter but is scoped to the variant, not the base doc.
/// </summary>
public class VariantSubParameter
{
    public int Id { get; set; }

    public int ParameterVariantId { get; set; }
    public ParameterVariant ParameterVariant { get; set; } = null!;

    public int StepOrder { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ScientificValueOwned Value { get; set; } = new();

    [MaxLength(60)]
    public string Unit { get; set; } = string.Empty;

    public string Rationale { get; set; } = string.Empty;

    [MaxLength(300)]
    public string SourceReference { get; set; } = string.Empty;

    /// <summary>
    /// Phase 3.5 — arithmetic operation this step applies to the running accumulator.
    /// Defaults to Multiply, preserving existing behaviour for all current scenarios.
    /// </summary>
    public StepOperation Operation { get; set; } = StepOperation.Multiply;
}