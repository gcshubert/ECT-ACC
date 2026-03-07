namespace ECT.ACC.Contracts.DTOs;

public record ProcessDomainDto(
    int    Id,
    string Name,
    string Description,
    string IconKey,
    IReadOnlyList<ParameterTemplateSummaryDto> Templates
);
