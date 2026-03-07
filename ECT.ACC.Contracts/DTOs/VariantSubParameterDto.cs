namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// Replaces the Phase 3 VariantSubParameterDto — adds Operation.
/// Drop this file and delete the Phase 3 VariantSubParameterDto.cs.
/// </summary>
public record VariantSubParameterDto(
    int                Id,
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    string             Unit,
    string             Rationale,
    string             SourceReference,
    StepOperationDto   Operation
);
