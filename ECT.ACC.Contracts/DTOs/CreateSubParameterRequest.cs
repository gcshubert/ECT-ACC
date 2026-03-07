namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// Replaces the Phase 3 CreateSubParameterRequest — adds Operation.
/// Drop this file and delete the Phase 3 CreateSubParameterRequest.cs.
/// </summary>
public record CreateSubParameterRequest(
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    string             Unit,
    string             Rationale,
    string             SourceReference,
    StepOperationDto   Operation = StepOperationDto.Multiply
);
