namespace ECT.ACC.Contracts.DTOs;

public record UpdateParameterDefinitionRequest(
    string              Symbol,
    string              Label,
    string              Description,
    string              Unit,
    int                 SortOrder,
    ScientificValueDto? DefaultValue
);
