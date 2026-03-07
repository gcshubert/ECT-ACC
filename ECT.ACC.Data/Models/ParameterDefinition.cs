using System.ComponentModel.DataAnnotations;
using ECT.ACC.Data.Math;

namespace ECT.ACC.Data.Models;

/// <summary>
/// A named domain category that groups related scenarios and provides
/// parameter templates.  Seeded at startup; users don't create these directly.
/// </summary>
public class ProcessDomain
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(40)]
    public string IconKey { get; set; } = string.Empty;

    public ICollection<ParameterTemplate> Templates { get; set; } = new List<ParameterTemplate>();
    public ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();
}

/// <summary>
/// A named starting configuration for a domain.  When a user creates a
/// Scenario from a template, its ParameterDefinitions are cloned from
/// TemplateParameterDefinitions.
/// </summary>
public class ParameterTemplate
{
    public int Id { get; set; }

    public int ProcessDomainId { get; set; }
    public ProcessDomain ProcessDomain { get; set; } = null!;

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public ICollection<TemplateParameterDefinition> ParameterDefinitions { get; set; }
        = new List<TemplateParameterDefinition>();
}

/// <summary>
/// One parameter slot in a template.  Cloned into ParameterDefinition
/// when a Scenario is created from this template.
/// </summary>
public class TemplateParameterDefinition
{
    public int Id { get; set; }

    public int ParameterTemplateId { get; set; }
    public ParameterTemplate ParameterTemplate { get; set; } = null!;

    [MaxLength(20)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Symbol { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(60)]
    public string DefaultUnit { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEctCoreParameter { get; set; }

    public ScientificValueOwned? SeedValue { get; set; }
}

/// <summary>
/// Defines one parameter slot for a specific Scenario.
/// For ECT core parameters the Key matches the existing parameter keys
/// ("e", "c", "k", "t").  Additional domain-specific parameters use
/// whatever key the domain template specifies.
/// </summary>
public class ParameterDefinition
{
    public int Id { get; set; }

    public int ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = null!;

    [MaxLength(20)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Symbol { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Unit { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEctCoreParameter { get; set; }

    public ScientificValueOwned? DefaultValue { get; set; }
}