namespace ECT.ACC.Contracts.DTOs;

public record ParameterDefinitionDto(
    int                 Id,
    string              Key,
    string              Symbol,
    string              Label,
    string              Description,
    string              Unit,
    int                 SortOrder,
    bool                IsEctCoreParameter,
    ScientificValueDto? DefaultValue
);
