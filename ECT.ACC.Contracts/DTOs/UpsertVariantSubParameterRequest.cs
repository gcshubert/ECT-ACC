namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// Replaces the Phase 3 UpsertVariantSubParameterRequest — adds Operation.
/// Drop this file and delete the Phase 3 UpsertVariantSubParameterRequest.cs.
/// </summary>
public record UpsertVariantSubParameterRequest(
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    string             Unit,
    string             Rationale,
    string             SourceReference,
    StepOperationDto   Operation = StepOperationDto.Multiply
);
