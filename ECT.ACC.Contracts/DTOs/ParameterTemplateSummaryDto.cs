namespace ECT.ACC.Contracts.DTOs;

public record ParameterTemplateSummaryDto(
    int    Id,
    string Name,
    string Description,
    IReadOnlyList<TemplateParameterDefinitionDto> ParameterDefinitions
);
