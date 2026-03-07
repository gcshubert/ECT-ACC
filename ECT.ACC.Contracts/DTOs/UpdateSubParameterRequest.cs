namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// Replaces the Phase 3 UpdateSubParameterRequest — adds Operation.
/// Drop this file and delete the Phase 3 UpdateSubParameterRequest.cs.
/// </summary>
public record UpdateSubParameterRequest(
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    string             Unit,
    string             Rationale,
    string             SourceReference,
    StepOperationDto   Operation = StepOperationDto.Multiply
);
