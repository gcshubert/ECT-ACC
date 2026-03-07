namespace ECT.ACC.Contracts.DTOs;

public record RollupStepDto(
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    StepOperationDto   Operation,
    ScientificValueDto RunningTotal
);
