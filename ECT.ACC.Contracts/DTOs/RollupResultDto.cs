namespace ECT.ACC.Contracts.DTOs;

public record RollupResultDto(
    string             ParameterKey,
    ScientificValueDto ComposedValue,
    IReadOnlyList<RollupStepDto> Steps
);
