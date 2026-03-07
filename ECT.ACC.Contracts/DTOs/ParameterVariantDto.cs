namespace ECT.ACC.Contracts.DTOs;

public record ParameterVariantDto(
    int    Id,
    string Name,
    bool   IsActive,
    IReadOnlyList<VariantSubParameterDto> SubParameters
);
