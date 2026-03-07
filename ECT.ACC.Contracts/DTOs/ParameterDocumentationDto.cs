namespace ECT.ACC.Contracts.DTOs;

public record ParameterDocumentationDto(
    int    Id,
    string ParameterKey,
    string Label,
    string DerivationNarrative,
    IReadOnlyList<SubParameterDto>     SubParameters,
    IReadOnlyList<ParameterVariantDto> Variants
);
