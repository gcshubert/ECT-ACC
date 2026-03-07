namespace ECT.ACC.Contracts.DTOs;

/// <summary>
/// Replaces the Phase 3 SubParameterDto — adds Operation.
/// Drop this file and delete the Phase 3 SubParameterDto.cs.
/// </summary>
public record SubParameterDto(
    int                Id,
    int                StepOrder,
    string             Name,
    ScientificValueDto Value,
    string             Unit,
    string             Rationale,
    string             SourceReference,
    StepOperationDto   Operation
);
