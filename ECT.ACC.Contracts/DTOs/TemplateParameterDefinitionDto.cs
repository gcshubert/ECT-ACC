namespace ECT.ACC.Contracts.DTOs;

public record TemplateParameterDefinitionDto(
    int                 Id,
    string              Key,
    string              Symbol,
    string              Label,
    string              Description,
    string              DefaultUnit,
    int                 SortOrder,
    bool                IsEctCoreParameter,
    ScientificValueDto? SeedValue
);
