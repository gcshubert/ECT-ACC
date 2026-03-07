namespace ECT.ACC.Contracts.DTOs;

public record CreateParameterDefinitionRequest(
    string              Key,
    string              Symbol,
    string              Label,
    string              Description,
    string              Unit,
    int                 SortOrder,
    bool                IsEctCoreParameter,
    ScientificValueDto? DefaultValue
);
